// <copyright file="SixDegreesPlugin.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Controller;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Model.Drawing;
    using MediaBrowser.Model.Logging;
    using MediaBrowser.Model.Plugins;
    using MediaBrowser.Model.Serialization;
    using SixDegrees.Configuration;

    /// <summary>
    /// The Six Degrees of Separation Plugin for Emby.
    /// </summary>
    public class SixDegreesPlugin : BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages
    {
        private readonly IServerApplicationHost applicationHost;
        private readonly ILibraryManager libraryManager;
        private readonly ILogger logger;
        private readonly IJsonSerializer jsonSerializer;
        private readonly Services.RelationshipGraph relationshipGraph;
        private readonly Services.PathfindingService pathfindingService;
        private readonly Services.RelationshipGraphService graphService;

        /// <summary>
        /// The Plugin ID.
        /// </summary>
        private readonly Guid id = new Guid("6D6D6D6D-5D5D-4D4D-3D3D-2D2D2D2D2D2D");

        /// <summary>
        /// Initializes a new instance of the <see cref="SixDegreesPlugin" /> class.
        /// </summary>
        /// <param name="applicationPaths">The application paths.</param>
        /// <param name="xmlSerializer">The XML serializer.</param>
        /// <param name="applicationHost">The application host.</param>
        /// <param name="libraryManager">The library manager.</param>
        /// <param name="logManager">The log manager.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        public SixDegreesPlugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            IServerApplicationHost applicationHost,
            ILibraryManager libraryManager,
            ILogManager logManager,
            IJsonSerializer jsonSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            this.applicationHost = applicationHost;
            this.libraryManager = libraryManager;
            this.logger = logManager.GetLogger(this.Name);
            this.jsonSerializer = jsonSerializer;

            Instance = this;

            // Initialize services
            this.relationshipGraph = new Services.RelationshipGraph(this.logger);
            this.pathfindingService = new Services.PathfindingService(this.logger, this.relationshipGraph);

            // Get data path for cache storage
            var dataPath = Path.Combine(applicationPaths.PluginConfigurationsPath, "SixDegrees");
            this.graphService = new Services.RelationshipGraphService(
                this.libraryManager,
                this.logger,
                this.jsonSerializer,
                this.relationshipGraph,
                () => this.Configuration,
                dataPath);

            // Load cache on startup
            this.logger.Info("Loading graph cache on startup...");
            if (!this.graphService.LoadCache())
            {
                this.logger.Info("Cache load failed or not found. Graph will need to be built.");
            }

            this.logger.Info("Six Degrees Plugin initialized");
        }

        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public static SixDegreesPlugin Instance { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public override string Description => "Interactive force-directed graph showing relationship connections between people across all media in your library.";

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        public override Guid Id => this.id;

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public override string Name => "SixDegrees";

        /// <summary>
        /// Gets the library manager.
        /// </summary>
        public ILibraryManager LibraryManager => this.libraryManager;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger => this.logger;

        /// <summary>
        /// Gets the JSON serializer.
        /// </summary>
        public IJsonSerializer JsonSerializer => this.jsonSerializer;

        /// <summary>
        /// Gets the relationship graph.
        /// </summary>
        public Services.RelationshipGraph RelationshipGraph => this.relationshipGraph;

        /// <summary>
        /// Gets the pathfinding service.
        /// </summary>
        public Services.PathfindingService PathfindingService => this.pathfindingService;

        /// <summary>
        /// Gets the graph service.
        /// </summary>
        public Services.RelationshipGraphService GraphService => this.graphService;

        /// <summary>
        /// Gets the thumb image format.
        /// </summary>
        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        /// <summary>
        /// Gets the thumb image.
        /// </summary>
        /// <returns>An image stream.</returns>
        public Stream GetThumbImage()
        {
            var type = this.GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }

        /// <summary>
        /// Gets the plugin pages.
        /// </summary>
        /// <returns>An enumerable collection of plugin page information.</returns>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "sixdegrees",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.sixdegrees.html",
                    EnableInMainMenu = true
                },
                new PluginPageInfo
                {
                    Name = "sixdegrees.js",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.sixdegrees.js"
                }
            };
        }
    }
}
