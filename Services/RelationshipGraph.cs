// <copyright file="RelationshipGraph.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MediaBrowser.Model.Logging;
    using SixDegrees.Models;

    /// <summary>
    /// Manages the relationship graph between people and media items.
    /// </summary>
    public class RelationshipGraph
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, PersonNode> people;
        private readonly Dictionary<string, MediaNode> media;
        private readonly object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraph"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public RelationshipGraph(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.people = new Dictionary<string, PersonNode>();
            this.media = new Dictionary<string, MediaNode>();
        }

        /// <summary>
        /// Gets the total number of people in the graph.
        /// </summary>
        public int PeopleCount => this.people.Count;

        /// <summary>
        /// Gets the total number of media items in the graph.
        /// </summary>
        public int MediaCount => this.media.Count;

        /// <summary>
        /// Gets the total number of connections in the graph.
        /// </summary>
        public int ConnectionCount
        {
            get
            {
                return this.people.Values.Sum(p => p.MediaConnections.Count);
            }
        }

        /// <summary>
        /// Adds a person to the graph.
        /// </summary>
        /// <param name="person">The person node to add.</param>
        public void AddPerson(PersonNode person)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            if (string.IsNullOrEmpty(person.Id))
            {
                throw new ArgumentException("Person ID cannot be null or empty.", nameof(person));
            }

            lock (this.lockObject)
            {
                if (!this.people.ContainsKey(person.Id))
                {
                    this.people[person.Id] = person;
                    this.logger.Debug($"Added person: {person.Name} (ID: {person.Id})");
                }
            }
        }

        /// <summary>
        /// Adds a media item to the graph.
        /// </summary>
        /// <param name="mediaItem">The media node to add.</param>
        public void AddMedia(MediaNode mediaItem)
        {
            if (mediaItem == null)
            {
                throw new ArgumentNullException(nameof(mediaItem));
            }

            if (string.IsNullOrEmpty(mediaItem.Id))
            {
                throw new ArgumentException("Media ID cannot be null or empty.", nameof(mediaItem));
            }

            lock (this.lockObject)
            {
                if (!this.media.ContainsKey(mediaItem.Id))
                {
                    this.media[mediaItem.Id] = mediaItem;
                    this.logger.Debug($"Added media: {mediaItem.Name} (ID: {mediaItem.Id})");
                }
            }
        }

        /// <summary>
        /// Creates a bidirectional connection between a person and a media item.
        /// </summary>
        /// <param name="personId">The person ID.</param>
        /// <param name="mediaId">The media ID.</param>
        /// <param name="role">The role (Actor, Director, Writer, etc.).</param>
        public void AddConnection(string personId, string mediaId, string role)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentException("Person ID cannot be null or empty.", nameof(personId));
            }

            if (string.IsNullOrEmpty(mediaId))
            {
                throw new ArgumentException("Media ID cannot be null or empty.", nameof(mediaId));
            }

            lock (this.lockObject)
            {
                if (!this.people.TryGetValue(personId, out var person))
                {
                    this.logger.Warn($"Person not found: {personId}");
                    return;
                }

                if (!this.media.TryGetValue(mediaId, out var mediaItem))
                {
                    this.logger.Warn($"Media not found: {mediaId}");
                    return;
                }

                // Add connection from person to media
                if (!person.MediaConnections.ContainsKey(mediaId))
                {
                    person.MediaConnections[mediaId] = new MediaConnection
                    {
                        MediaId = mediaItem.Id,
                        MediaName = mediaItem.Name,
                        MediaType = mediaItem.MediaType,
                        Role = role,
                        Year = mediaItem.Year,
                        ImageUrl = mediaItem.ImageUrl
                    };
                }

                // Add connection from media to person
                if (!mediaItem.PeopleConnections.ContainsKey(personId))
                {
                    mediaItem.PeopleConnections[personId] = new PersonConnection
                    {
                        PersonId = person.Id,
                        PersonName = person.Name,
                        Role = role,
                        ImageUrl = person.ImageUrl
                    };
                }

                this.logger.Debug($"Connected {person.Name} to {mediaItem.Name} as {role}");
            }
        }

        /// <summary>
        /// Gets a person by ID.
        /// </summary>
        /// <param name="personId">The person ID.</param>
        /// <returns>The person node, or null if not found.</returns>
        public PersonNode GetPerson(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                return null;
            }

            lock (this.lockObject)
            {
                return this.people.TryGetValue(personId, out var person) ? person : null;
            }
        }

        /// <summary>
        /// Gets a media item by ID.
        /// </summary>
        /// <param name="mediaId">The media ID.</param>
        /// <returns>The media node, or null if not found.</returns>
        public MediaNode GetMedia(string mediaId)
        {
            if (string.IsNullOrEmpty(mediaId))
            {
                return null;
            }

            lock (this.lockObject)
            {
                return this.media.TryGetValue(mediaId, out var mediaItem) ? mediaItem : null;
            }
        }

        /// <summary>
        /// Gets all media connections for a person.
        /// </summary>
        /// <param name="personId">The person ID.</param>
        /// <returns>A collection of media connections.</returns>
        public IEnumerable<MediaConnection> GetPersonMediaConnections(string personId)
        {
            var person = this.GetPerson(personId);
            return person?.MediaConnections.Values ?? Enumerable.Empty<MediaConnection>();
        }

        /// <summary>
        /// Gets all people connections for a media item.
        /// </summary>
        /// <param name="mediaId">The media ID.</param>
        /// <returns>A collection of person connections.</returns>
        public IEnumerable<PersonConnection> GetMediaPeopleConnections(string mediaId)
        {
            var mediaItem = this.GetMedia(mediaId);
            return mediaItem?.PeopleConnections.Values ?? Enumerable.Empty<PersonConnection>();
        }

        /// <summary>
        /// Clears all data from the graph.
        /// </summary>
        public void Clear()
        {
            lock (this.lockObject)
            {
                this.people.Clear();
                this.media.Clear();
                this.logger.Info("Graph cleared");
            }
        }

        /// <summary>
        /// Gets graph statistics.
        /// </summary>
        /// <returns>A dictionary containing graph statistics.</returns>
        public Dictionary<string, object> GetStatistics()
        {
            lock (this.lockObject)
            {
                return new Dictionary<string, object>
                {
                    { "peopleCount", this.PeopleCount },
                    { "mediaCount", this.MediaCount },
                    { "connectionCount", this.ConnectionCount },
                    { "averageConnectionsPerPerson", this.PeopleCount > 0 ? (double)this.ConnectionCount / this.PeopleCount : 0 }
                };
            }
        }
    }
}
