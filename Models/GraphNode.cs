// <copyright file="GraphNode.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a node in the relationship graph (either a person or media item).
    /// </summary>
    public class GraphNode
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the node type (Person or Media).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the media type (Movie, Series, Album) - only for media nodes.
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the depth (degrees of separation from the origin node).
        /// </summary>
        public int? Depth { get; set; }

        /// <summary>
        /// Gets or sets the connections to other nodes.
        /// </summary>
        public List<Connection> Connections { get; set; } = new List<Connection>();
    }

    /// <summary>
    /// Represents a connection/edge between two nodes.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Gets or sets the target node ID.
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// Gets or sets the role/relationship type.
        /// </summary>
        public string Role { get; set; }
    }

    /// <summary>
    /// Represents a person node in the graph.
    /// </summary>
    public class PersonNode
    {
        /// <summary>
        /// Gets or sets the person ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the person name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the media connections.
        /// </summary>
        public Dictionary<string, MediaConnection> MediaConnections { get; set; } = new Dictionary<string, MediaConnection>();
    }

    /// <summary>
    /// Represents a connection from a person to a media item.
    /// </summary>
    public class MediaConnection
    {
        /// <summary>
        /// Gets or sets the media ID.
        /// </summary>
        public string MediaId { get; set; }

        /// <summary>
        /// Gets or sets the media name.
        /// </summary>
        public string MediaName { get; set; }

        /// <summary>
        /// Gets or sets the media type (Movie, Series, Album).
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the role (Actor, Director, Writer, etc.).
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }
    }

    /// <summary>
    /// Represents a media node in the graph.
    /// </summary>
    public class MediaNode
    {
        /// <summary>
        /// Gets or sets the media ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the media name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the media type.
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the people connections.
        /// </summary>
        public Dictionary<string, PersonConnection> PeopleConnections { get; set; } = new Dictionary<string, PersonConnection>();
    }

    /// <summary>
    /// Represents a connection from a media item to a person.
    /// </summary>
    public class PersonConnection
    {
        /// <summary>
        /// Gets or sets the person ID.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person name.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }
    }
}
