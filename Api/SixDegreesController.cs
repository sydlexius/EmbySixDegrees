// <copyright file="SixDegreesController.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MediaBrowser.Model.Logging;
    using MediaBrowser.Model.Services;
    using SixDegrees.Models;
    using SixDegrees.Services;

    /// <summary>
    /// API controller for Six Degrees plugin endpoints.
    /// Provides endpoints for searching people, finding connections, and managing the relationship graph.
    /// </summary>
    public class SixDegreesController : IService
    {
        private readonly ILogger logger;
        private readonly RelationshipGraph graph;
        private readonly PathfindingService pathfindingService;
        private readonly RelationshipGraphService graphService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SixDegreesController"/> class.
        /// </summary>
        public SixDegreesController()
        {
            var plugin = SixDegreesPlugin.Instance;
            this.logger = plugin.Logger;
            this.graph = plugin.RelationshipGraph;
            this.pathfindingService = plugin.PathfindingService;
            this.graphService = plugin.GraphService;
        }

        /// <summary>
        /// Gets graph statistics.
        /// </summary>
        /// <param name="request">The statistics request.</param>
        /// <returns>Graph statistics including people count, media count, and connections.</returns>
        public object Get(GetStatistics request)
        {
            this.logger.Info("GetStatistics endpoint called");
            var stats = this.graph.GetStatistics();
            return new StatisticsResponse
            {
                PeopleCount = stats.ContainsKey("peopleCount") ? Convert.ToInt32(stats["peopleCount"]) : 0,
                MediaCount = stats.ContainsKey("mediaCount") ? Convert.ToInt32(stats["mediaCount"]) : 0,
                ConnectionCount = stats.ContainsKey("connectionCount") ? Convert.ToInt32(stats["connectionCount"]) : 0,
                LastBuildTime = stats.ContainsKey("lastBuildTime") ? stats["lastBuildTime"]?.ToString() : null
            };
        }

        /// <summary>
        /// Searches for people by name.
        /// </summary>
        /// <param name="request">The search request containing query and pagination.</param>
        /// <returns>Search results with matching people.</returns>
        public object Get(SearchPeople request)
        {
            this.logger.Info($"SearchPeople endpoint called: Query='{request.Query}', Limit={request.Limit}");

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return new PersonSearchResponse
                {
                    Query = request.Query,
                    Limit = request.Limit,
                    Offset = 0,
                    Count = 0,
                    Results = new List<PersonSearchResult>()
                };
            }

            // Enforce limits
            var limit = Math.Max(1, Math.Min(request.Limit, 100));

            var results = this.graph.SearchPeople(request.Query, limit, 0);
            var resultList = results.ToList();

            return new PersonSearchResponse
            {
                Query = request.Query,
                Limit = limit,
                Offset = 0,
                Count = resultList.Count,
                Results = resultList.Select(p => new PersonSearchResult
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    ConnectionCount = p.MediaConnections.Count
                }).ToList()
            };
        }

        /// <summary>
        /// Gets graph data for visualization centered on a person.
        /// </summary>
        /// <param name="request">The graph request with person ID and depth.</param>
        /// <returns>Graph data with nodes and edges for D3.js visualization.</returns>
        public object Get(GetGraph request)
        {
            this.logger.Info($"GetGraph endpoint called: PersonId={request.PersonId}, Depth={request.Depth}");

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(request.PersonId))
            {
                return new GraphData
                {
                    Success = false,
                    Message = "PersonId is required"
                };
            }

            // Enforce depth limits (1-6)
            var depth = Math.Max(1, Math.Min(request.Depth, 6));

            var result = this.pathfindingService.GetNeighbors(request.PersonId, depth, 500);

            return new GraphData
            {
                Success = result.Success,
                Message = result.Message,
                PersonId = result.PersonId,
                Depth = result.Degree,
                Nodes = result.Nodes?.Select(n => new GraphNodeDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Type = n.Type,
                    MediaType = n.MediaType,
                    ImageUrl = n.ImageUrl,
                    Depth = n.Depth
                }).ToList(),
                Edges = result.Edges?.Select(e => new GraphEdgeDto
                {
                    Source = e.Source,
                    Target = e.Target,
                    Role = e.Role
                }).ToList(),
                Truncated = result.Truncated,
                SearchTimeMs = result.SearchTimeMs,
                NodesVisited = result.NodesVisited
            };
        }

        /// <summary>
        /// Finds the shortest path between two people.
        /// </summary>
        /// <param name="request">The pathfinding request with source and target person IDs.</param>
        /// <returns>The shortest path between the two people if one exists.</returns>
        public object Get(GetShortestPath request)
        {
            this.logger.Info($"GetShortestPath endpoint called: {request.FromPersonId} -> {request.ToPersonId}");

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(request.FromPersonId))
            {
                return new PathResultDto
                {
                    Success = false,
                    Message = "FromPersonId is required"
                };
            }

            if (string.IsNullOrWhiteSpace(request.ToPersonId))
            {
                return new PathResultDto
                {
                    Success = false,
                    Message = "ToPersonId is required"
                };
            }

            var result = this.pathfindingService.FindShortestPath(request.FromPersonId, request.ToPersonId);

            return new PathResultDto
            {
                Success = result.Success,
                Message = result.Message,
                Path = result.Path?.Select(n => new PathNodeDto
                {
                    Type = n.Type,
                    Id = n.Id,
                    Name = n.Name,
                    MediaType = n.MediaType,
                    ImageUrl = n.ImageUrl,
                    Role = n.Role
                }).ToList(),
                Degrees = result.Degrees,
                SearchTimeMs = result.SearchTimeMs,
                NodesVisited = result.NodesVisited
            };
        }

        /// <summary>
        /// Gets N-degree neighbors for a person (extended graph endpoint).
        /// </summary>
        /// <param name="request">The neighbors request.</param>
        /// <returns>The neighbors result.</returns>
        public object Get(GetNeighbors request)
        {
            this.logger.Info($"GetNeighbors endpoint called: {request.PersonId}, Degree: {request.Degree}, MaxNodes: {request.MaxNodes}");

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(request.PersonId))
            {
                return new GraphData
                {
                    Success = false,
                    Message = "PersonId is required"
                };
            }

            var result = this.pathfindingService.GetNeighbors(request.PersonId, request.Degree, request.MaxNodes);

            return new GraphData
            {
                Success = result.Success,
                Message = result.Message,
                PersonId = result.PersonId,
                Depth = result.Degree,
                Nodes = result.Nodes?.Select(n => new GraphNodeDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Type = n.Type,
                    MediaType = n.MediaType,
                    ImageUrl = n.ImageUrl,
                    Depth = n.Depth
                }).ToList(),
                Edges = result.Edges?.Select(e => new GraphEdgeDto
                {
                    Source = e.Source,
                    Target = e.Target,
                    Role = e.Role
                }).ToList(),
                Truncated = result.Truncated,
                SearchTimeMs = result.SearchTimeMs,
                NodesVisited = result.NodesVisited
            };
        }

        /// <summary>
        /// Gets all people with pagination.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Paginated list of people.</returns>
        public object Get(GetPeople request)
        {
            this.logger.Info($"GetPeople endpoint called: Limit={request.Limit}, Offset={request.Offset}");

            // Enforce limits
            var limit = Math.Max(1, Math.Min(request.Limit, 200));
            var offset = Math.Max(0, request.Offset);

            var results = this.graph.GetAllPeople(limit, offset);
            var resultList = results.ToList();

            return new PeopleListResponse
            {
                Limit = limit,
                Offset = offset,
                Count = resultList.Count,
                TotalCount = this.graph.PeopleCount,
                Results = resultList.Select(p => new PersonSearchResult
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    ConnectionCount = p.MediaConnections.Count
                }).ToList()
            };
        }

        /// <summary>
        /// Triggers a cache rebuild.
        /// </summary>
        /// <param name="request">The rebuild request.</param>
        /// <returns>The cache status after rebuild.</returns>
        public object Post(RebuildCache request)
        {
            this.logger.Info("RebuildCache endpoint called");

            try
            {
                var result = this.graphService.BuildGraph();
                return new CacheStatusResult
                {
                    Success = result.Success,
                    Message = result.Message,
                    PeopleCount = result.PeopleCount,
                    MediaCount = result.MediaCount,
                    ConnectionCount = result.ConnectionCount,
                    BuildTimeMs = result.BuildTimeMs
                };
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error rebuilding cache: {ex.Message}", ex);
                return new CacheStatusResult
                {
                    Success = false,
                    Message = $"Error rebuilding cache: {ex.Message}"
                };
            }
        }
    }

    #region Request DTOs

    /// <summary>
    /// Request for getting graph statistics.
    /// </summary>
    [Route("/SixDegrees/Statistics", "GET", Summary = "Get graph statistics", Description = "Returns statistics about the relationship graph including people count, media count, and total connections.")]
    public class GetStatistics : IReturn<StatisticsResponse>
    {
    }

    /// <summary>
    /// Search for people by name.
    /// </summary>
    [Route("/SixDegrees/SearchPeople", "GET", Summary = "Search for people by name", Description = "Searches the relationship graph for people matching the query string.")]
    public class SearchPeople : IReturn<PersonSearchResponse>
    {
        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        [ApiMember(Name = "Query", Description = "Search query string to match against person names", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results (default: 20, max: 100).
        /// </summary>
        [ApiMember(Name = "Limit", Description = "Maximum number of results to return (default: 20, max: 100)", IsRequired = false, DataType = "int", ParameterType = "query")]
        public int Limit { get; set; } = 20;
    }

    /// <summary>
    /// Get graph data for visualization.
    /// </summary>
    [Route("/SixDegrees/Graph", "GET", Summary = "Get graph data for visualization", Description = "Returns graph data (nodes and edges) centered on a person for D3.js visualization.")]
    public class GetGraph : IReturn<GraphData>
    {
        /// <summary>
        /// Gets or sets the starting person ID.
        /// </summary>
        [ApiMember(Name = "PersonId", Description = "The ID of the person to center the graph on", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the degrees of separation (1-6).
        /// </summary>
        [ApiMember(Name = "Depth", Description = "Degrees of separation to include (1-6, default: 2)", IsRequired = false, DataType = "int", ParameterType = "query")]
        public int Depth { get; set; } = 2;
    }

    /// <summary>
    /// Find shortest path between two people.
    /// </summary>
    [Route("/SixDegrees/ShortestPath", "GET", Summary = "Find shortest path between two people", Description = "Uses BFS algorithm to find the shortest connection path between two people through shared media.")]
    public class GetShortestPath : IReturn<PathResultDto>
    {
        /// <summary>
        /// Gets or sets the source person ID.
        /// </summary>
        [ApiMember(Name = "FromPersonId", Description = "The ID of the starting person", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string FromPersonId { get; set; }

        /// <summary>
        /// Gets or sets the target person ID.
        /// </summary>
        [ApiMember(Name = "ToPersonId", Description = "The ID of the target person", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string ToPersonId { get; set; }
    }

    /// <summary>
    /// Get N-degree neighbors for a person.
    /// </summary>
    [Route("/SixDegrees/Neighbors", "GET", Summary = "Get N-degree neighbors", Description = "Expands the graph from a person to N degrees of separation with configurable node limits.")]
    public class GetNeighbors : IReturn<GraphData>
    {
        /// <summary>
        /// Gets or sets the person ID.
        /// </summary>
        [ApiMember(Name = "PersonId", Description = "The ID of the person to expand from", IsRequired = true, DataType = "string", ParameterType = "query")]
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the degree (1-6).
        /// </summary>
        [ApiMember(Name = "Degree", Description = "Degrees of separation to expand (1-6, default: 2)", IsRequired = false, DataType = "int", ParameterType = "query")]
        public int Degree { get; set; } = 2;

        /// <summary>
        /// Gets or sets the maximum number of nodes to return (default: 500, max: 1000).
        /// </summary>
        [ApiMember(Name = "MaxNodes", Description = "Maximum number of nodes to return (default: 500, max: 1000)", IsRequired = false, DataType = "int", ParameterType = "query")]
        public int MaxNodes { get; set; } = 500;
    }

    /// <summary>
    /// Get all people with pagination.
    /// </summary>
    [Route("/SixDegrees/People", "GET", Summary = "Get all people", Description = "Returns a paginated list of all people in the relationship graph.")]
    public class GetPeople : IReturn<PeopleListResponse>
    {
        /// <summary>
        /// Gets or sets the maximum number of results (default: 50, max: 200).
        /// </summary>
        [ApiMember(Name = "Limit", Description = "Maximum number of results to return (default: 50, max: 200)", IsRequired = false, DataType = "int", ParameterType = "query")]
        public int Limit { get; set; } = 50;

        /// <summary>
        /// Gets or sets the number of results to skip (default: 0).
        /// </summary>
        [ApiMember(Name = "Offset", Description = "Number of results to skip for pagination (default: 0)", IsRequired = false, DataType = "int", ParameterType = "query")]
        public int Offset { get; set; } = 0;
    }

    /// <summary>
    /// Trigger cache rebuild.
    /// </summary>
    [Route("/SixDegrees/RebuildCache", "POST", Summary = "Trigger cache rebuild", Description = "Rebuilds the relationship graph cache by scanning the entire media library.")]
    public class RebuildCache : IReturn<CacheStatusResult>
    {
    }

    #endregion
}
