// <copyright file="ApiModels.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Api
{
    using System.Collections.Generic;

    /// <summary>
    /// Result of a person search operation.
    /// </summary>
    public class PersonSearchResult
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
        /// Gets or sets the number of media connections.
        /// </summary>
        public int ConnectionCount { get; set; }
    }

    /// <summary>
    /// Response for person search endpoint.
    /// </summary>
    public class PersonSearchResponse
    {
        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the count of results returned.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the search results.
        /// </summary>
        public List<PersonSearchResult> Results { get; set; }
    }

    /// <summary>
    /// Response for people list endpoint.
    /// </summary>
    public class PeopleListResponse
    {
        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the count of results returned.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the total count of people.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the people results.
        /// </summary>
        public List<PersonSearchResult> Results { get; set; }
    }

    /// <summary>
    /// Graph data for visualization.
    /// </summary>
    public class GraphData
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if unsuccessful.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the center person ID.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the degree of separation.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Gets or sets the graph nodes.
        /// </summary>
        public List<GraphNodeDto> Nodes { get; set; }

        /// <summary>
        /// Gets or sets the graph edges.
        /// </summary>
        public List<GraphEdgeDto> Edges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether results were truncated.
        /// </summary>
        public bool Truncated { get; set; }

        /// <summary>
        /// Gets or sets the search time in milliseconds.
        /// </summary>
        public double SearchTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the number of nodes visited.
        /// </summary>
        public int NodesVisited { get; set; }
    }

    /// <summary>
    /// Node in graph visualization data.
    /// </summary>
    public class GraphNodeDto
    {
        /// <summary>
        /// Gets or sets the node ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the node name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the node type (person or media).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the media type (Movie, Series, Album).
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the depth (degrees from center).
        /// </summary>
        public int? Depth { get; set; }
    }

    /// <summary>
    /// Edge in graph visualization data.
    /// </summary>
    public class GraphEdgeDto
    {
        /// <summary>
        /// Gets or sets the source node ID.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the target node ID.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the role/relationship type.
        /// </summary>
        public string Role { get; set; }
    }

    /// <summary>
    /// Result of shortest path operation.
    /// </summary>
    public class PathResultDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the path nodes.
        /// </summary>
        public List<PathNodeDto> Path { get; set; }

        /// <summary>
        /// Gets or sets the degrees of separation.
        /// </summary>
        public int Degrees { get; set; }

        /// <summary>
        /// Gets or sets the search time in milliseconds.
        /// </summary>
        public double SearchTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the number of nodes visited.
        /// </summary>
        public int NodesVisited { get; set; }
    }

    /// <summary>
    /// Node in a path result.
    /// </summary>
    public class PathNodeDto
    {
        /// <summary>
        /// Gets or sets the node type (person or media).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the node ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the node name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the media type (if applicable).
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the role (for media nodes).
        /// </summary>
        public string Role { get; set; }
    }

    /// <summary>
    /// Result of cache rebuild operation.
    /// </summary>
    public class CacheStatusResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message.
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

    /// <summary>
    /// Graph statistics response.
    /// </summary>
    public class StatisticsResponse
    {
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
        /// Gets or sets the last build time.
        /// </summary>
        public string LastBuildTime { get; set; }
    }
}
