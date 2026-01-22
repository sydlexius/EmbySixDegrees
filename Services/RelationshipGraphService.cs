// <copyright file="RelationshipGraphService.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Model.Dto;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.Logging;
    using MediaBrowser.Model.Querying;
    using MediaBrowser.Model.Serialization;
    using SixDegrees.Configuration;
    using SixDegrees.Models;

    /// <summary>
    /// Service for scanning the Emby library and building the relationship graph cache.
    /// </summary>
    public class RelationshipGraphService
    {
        private readonly ILibraryManager libraryManager;
        private readonly ILogger logger;
        private readonly IJsonSerializer jsonSerializer;
        private readonly RelationshipGraph graph;
        private readonly Func<PluginConfiguration> getConfig;
        private readonly string cacheFilePath;
        private DateTime? lastBuildTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraphService"/> class.
        /// </summary>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="graph">The relationship graph instance.</param>
        /// <param name="getConfig">Function to get the plugin configuration.</param>
        /// <param name="dataPath">The data directory path for cache storage.</param>
        public RelationshipGraphService(
            ILibraryManager libraryManager,
            ILogger logger,
            IJsonSerializer jsonSerializer,
            RelationshipGraph graph,
            Func<PluginConfiguration> getConfig,
            string dataPath)
        {
            this.libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
            this.getConfig = getConfig ?? throw new ArgumentNullException(nameof(getConfig));

            if (string.IsNullOrEmpty(dataPath))
            {
                throw new ArgumentException("Data path cannot be null or empty.", nameof(dataPath));
            }

            // Ensure data directory exists
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            this.cacheFilePath = Path.Combine(dataPath, "graph-cache.json");
        }

        /// <summary>
        /// Gets the last time the graph was built.
        /// </summary>
        public DateTime? LastBuildTime => this.lastBuildTime;

        /// <summary>
        /// Builds the relationship graph by scanning the entire library.
        /// </summary>
        /// <returns>A result indicating success or failure with statistics.</returns>
        public BuildResult BuildGraph()
        {
            var startTime = DateTime.UtcNow;
            this.logger.Info("Starting graph build...");

            try
            {
                // Clear existing graph
                this.graph.Clear();

                // Get all items from library
                var items = this.GetLibraryItems();
                var totalItems = items.Count;

                if (totalItems == 0)
                {
                    this.logger.Warn("No media items found in library");
                    return new BuildResult
                    {
                        Success = true,
                        Message = "No media items found in library",
                        PeopleCount = 0,
                        MediaCount = 0,
                        ConnectionCount = 0,
                        BuildTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
                    };
                }

                this.logger.Info($"Found {totalItems} media items to process");

                // Track people we've already added
                var processedPeople = new HashSet<string>();
                var processedItems = 0;
                var lastProgressLog = 0;

                // Process each item
                foreach (var item in items)
                {
                    processedItems++;

                    // Log progress every 10%
                    var progressPercent = (processedItems * 100) / totalItems;
                    if (progressPercent >= lastProgressLog + 10)
                    {
                        this.logger.Info($"Progress: {progressPercent}% ({processedItems}/{totalItems} items processed)");
                        lastProgressLog = progressPercent;
                    }

                    try
                    {
                        this.ProcessMediaItem(item, processedPeople);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error($"Error processing item {item.Name} (ID: {item.Id}): {ex.Message}");
                    }
                }

                // Save cache to disk
                this.SaveCache();

                this.lastBuildTime = DateTime.UtcNow;
                var buildTimeMs = (this.lastBuildTime.Value - startTime).TotalMilliseconds;

                var stats = this.graph.GetStatistics();
                this.logger.Info($"Graph build completed in {buildTimeMs:F0}ms - People: {stats["peopleCount"]}, Media: {stats["mediaCount"]}, Connections: {stats["connectionCount"]}");

                return new BuildResult
                {
                    Success = true,
                    Message = "Graph built successfully",
                    PeopleCount = (int)stats["peopleCount"],
                    MediaCount = (int)stats["mediaCount"],
                    ConnectionCount = (int)stats["connectionCount"],
                    BuildTimeMs = buildTimeMs
                };
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error building graph: {ex.Message}", ex);
                return new BuildResult
                {
                    Success = false,
                    Message = $"Error building graph: {ex.Message}",
                    PeopleCount = 0,
                    MediaCount = 0,
                    ConnectionCount = 0,
                    BuildTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
        }

        /// <summary>
        /// Loads the graph from disk cache if it exists and is valid.
        /// </summary>
        /// <returns>True if cache was loaded successfully, false otherwise.</returns>
        public bool LoadCache()
        {
            if (!File.Exists(this.cacheFilePath))
            {
                this.logger.Info("No cache file found");
                return false;
            }

            try
            {
                this.logger.Info("Loading graph from cache...");
                var startTime = DateTime.UtcNow;

                var json = File.ReadAllText(this.cacheFilePath);
                var cache = this.jsonSerializer.DeserializeFromString<GraphCache>(json);

                if (cache == null)
                {
                    this.logger.Warn("Cache file is empty or invalid");
                    return false;
                }

                // Validate cache
                if (!this.ValidateCache(cache))
                {
                    this.logger.Warn("Cache validation failed");
                    return false;
                }

                // Load into graph
                this.graph.Clear();

                foreach (var person in cache.People)
                {
                    this.graph.AddPerson(person);
                }

                foreach (var media in cache.Media)
                {
                    this.graph.AddMedia(media);
                }

                this.lastBuildTime = cache.BuildTime;
                var loadTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

                this.logger.Info($"Cache loaded successfully in {loadTimeMs:F0}ms - People: {cache.People.Count}, Media: {cache.Media.Count}");
                return true;
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error loading cache: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Saves the current graph to disk cache.
        /// </summary>
        public void SaveCache()
        {
            try
            {
                this.logger.Info("Saving graph to cache...");
                var startTime = DateTime.UtcNow;

                var cache = new GraphCache
                {
                    BuildTime = DateTime.UtcNow,
                    People = this.graph.GetAllPeople(int.MaxValue, 0).ToList(),
                    Media = this.GetAllMedia().ToList()
                };

                var json = this.jsonSerializer.SerializeToString(cache);
                File.WriteAllText(this.cacheFilePath, json);

                var saveTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var fileSizeKb = new FileInfo(this.cacheFilePath).Length / 1024;
                this.logger.Info($"Cache saved successfully in {saveTimeMs:F0}ms - Size: {fileSizeKb}KB");
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error saving cache: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if the cache needs to be rebuilt based on age and configuration.
        /// </summary>
        /// <returns>True if cache should be rebuilt, false otherwise.</returns>
        public bool ShouldRebuildCache()
        {
            if (!this.lastBuildTime.HasValue)
            {
                return true;
            }

            var cacheAge = DateTime.UtcNow - this.lastBuildTime.Value;
            var maxAge = TimeSpan.FromMinutes(this.getConfig().CacheRefreshIntervalMinutes);

            return cacheAge > maxAge;
        }

        /// <summary>
        /// Gets all library items based on configuration.
        /// </summary>
        /// <returns>A list of base items.</returns>
        private List<BaseItem> GetLibraryItems()
        {
            var includeTypes = new List<string>();
            var config = this.getConfig();

            if (config.IncludeMovies)
            {
                includeTypes.Add("Movie");
            }

            if (config.IncludeTV)
            {
                includeTypes.Add("Series");
            }

            if (config.IncludeMusic)
            {
                includeTypes.Add("MusicAlbum");
            }

            if (includeTypes.Count == 0)
            {
                this.logger.Warn("No media types enabled in configuration");
                return new List<BaseItem>();
            }

            var query = new InternalItemsQuery
            {
                IncludeItemTypes = includeTypes.ToArray(),
                Recursive = true
            };

            return this.libraryManager.GetItemList(query).ToList();
        }

        /// <summary>
        /// Processes a single media item and adds it to the graph.
        /// </summary>
        /// <param name="item">The media item to process.</param>
        /// <param name="processedPeople">Set of people IDs already processed.</param>
        private void ProcessMediaItem(BaseItem item, HashSet<string> processedPeople)
        {
            if (item == null)
            {
                return;
            }

            // Determine media type
            string mediaType;
            if (item is MediaBrowser.Controller.Entities.Movies.Movie)
            {
                mediaType = "Movie";
            }
            else if (item is MediaBrowser.Controller.Entities.TV.Series)
            {
                mediaType = "Series";
            }
            else if (item is MediaBrowser.Controller.Entities.Audio.MusicAlbum)
            {
                mediaType = "Album";
            }
            else
            {
                mediaType = "Unknown";
            }

            // Extract year
            int? year = item.ProductionYear;

            // Get image URL
            var imageUrl = this.GetImageUrl(item);

            // Create media node
            var mediaNode = new MediaNode
            {
                Id = item.Id.ToString(),
                Name = item.Name,
                MediaType = mediaType,
                Year = year,
                ImageUrl = imageUrl
            };

            this.graph.AddMedia(mediaNode);

            // Use GetItemPeople to get the people with roles for this media item
            var peopleQuery = new InternalPeopleQuery
            {
                ItemIds = new[] { item.InternalId }
            };
            var peopleResult = this.libraryManager.GetItemPeople(peopleQuery);

            if (peopleResult == null || peopleResult.Count == 0)
            {
                return;
            }

            foreach (var person in peopleResult)
            {
                if (string.IsNullOrEmpty(person.Name))
                {
                    continue;
                }

                // Use ItemId as the person ID, or Name if ItemId is 0
                var personId = person.ItemId > 0 ? person.ItemId.ToString() : person.Name;

                // Add person if not already processed
                if (!processedPeople.Contains(personId))
                {
                    var personNode = new PersonNode
                    {
                        Id = personId,
                        Name = person.Name,
                        ImageUrl = person.ItemId > 0 ? $"/emby/Items/{person.ItemId}/Images/Primary" : null
                    };

                    this.graph.AddPerson(personNode);
                    processedPeople.Add(personId);
                }

                // Convert PersonType enum to string for the role
                var role = person.Type.ToString();
                this.graph.AddConnection(personId, mediaNode.Id, role);
            }
        }

        /// <summary>
        /// Gets the image URL for a media item.
        /// </summary>
        /// <param name="item">The media item.</param>
        /// <returns>The image URL or null.</returns>
        private string GetImageUrl(BaseItem item)
        {
            if (item.HasImage(MediaBrowser.Model.Entities.ImageType.Primary))
            {
                return $"/emby/Items/{item.Id}/Images/Primary";
            }

            return null;
        }

        /// <summary>
        /// Gets the image URL for a person.
        /// </summary>
        /// <param name="personInfo">The person info.</param>
        /// <returns>The image URL or null.</returns>
        private string GetPersonImageUrl(BaseItemPerson personInfo)
        {
            // BaseItemPerson doesn't have ImageUrl property, construct from ID
            if (!string.IsNullOrEmpty(personInfo.Id))
            {
                return $"/emby/Items/{personInfo.Id}/Images/Primary";
            }

            return null;
        }

        /// <summary>
        /// Gets all media items from the graph.
        /// </summary>
        /// <returns>An enumerable of media nodes.</returns>
        private IEnumerable<MediaNode> GetAllMedia()
        {
            // We need to access the internal media dictionary
            // Since it's not exposed, we'll need to iterate through people's connections
            var mediaDict = new Dictionary<string, MediaNode>();

            foreach (var person in this.graph.GetAllPeople(int.MaxValue, 0))
            {
                foreach (var connection in person.MediaConnections.Values)
                {
                    var media = this.graph.GetMedia(connection.MediaId);
                    if (media != null && !mediaDict.ContainsKey(media.Id))
                    {
                        mediaDict[media.Id] = media;
                    }
                }
            }

            return mediaDict.Values;
        }

        /// <summary>
        /// Validates the cache data.
        /// </summary>
        /// <param name="cache">The cache to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        private bool ValidateCache(GraphCache cache)
        {
            if (cache == null)
            {
                return false;
            }

            // Check cache age
            var cacheAge = DateTime.UtcNow - cache.BuildTime;
            var maxAge = TimeSpan.FromMinutes(this.getConfig().CacheRefreshIntervalMinutes);

            if (cacheAge > maxAge)
            {
                this.logger.Info($"Cache is too old ({cacheAge.TotalMinutes:F0} minutes > {maxAge.TotalMinutes} minutes)");
                return false;
            }

            // Check item counts
            if (cache.People == null || cache.Media == null)
            {
                this.logger.Warn("Cache has null collections");
                return false;
            }

            if (cache.People.Count == 0 || cache.Media.Count == 0)
            {
                this.logger.Warn("Cache has empty collections");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Represents the cache data structure.
        /// </summary>
        private class GraphCache
        {
            /// <summary>
            /// Gets or sets the build time.
            /// </summary>
            public DateTime BuildTime { get; set; }

            /// <summary>
            /// Gets or sets the list of people.
            /// </summary>
            public List<PersonNode> People { get; set; }

            /// <summary>
            /// Gets or sets the list of media items.
            /// </summary>
            public List<MediaNode> Media { get; set; }
        }
    }

    /// <summary>
    /// Represents the result of a graph build operation.
    /// </summary>
    public class BuildResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the build was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the result message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the number of people in the graph.
        /// </summary>
        public int PeopleCount { get; set; }

        /// <summary>
        /// Gets or sets the number of media items in the graph.
        /// </summary>
        public int MediaCount { get; set; }

        /// <summary>
        /// Gets or sets the number of connections in the graph.
        /// </summary>
        public int ConnectionCount { get; set; }

        /// <summary>
        /// Gets or sets the build time in milliseconds.
        /// </summary>
        public double BuildTimeMs { get; set; }
    }
}
