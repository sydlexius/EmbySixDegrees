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
    /// </summary>
    public class SixDegreesController : IService
    {
        private readonly ILogger logger;
        private readonly RelationshipGraph graph;
        private readonly PathfindingService pathfindingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SixDegreesController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="graph">The relationship graph.</param>
        /// <param name="pathfindingService">The pathfinding service.</param>
        public SixDegreesController(ILogger logger, RelationshipGraph graph, PathfindingService pathfindingService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
            this.pathfindingService = pathfindingService ?? throw new ArgumentNullException(nameof(pathfindingService));
        }

        /// <summary>
        /// Gets graph statistics.
        /// </summary>
        /// <param name="request">The statistics request.</param>
        /// <returns>Graph statistics.</returns>
        public object Get(GetStatisticsRequest request)
        {
            this.logger.Info("GetStatistics endpoint called");
            return this.graph.GetStatistics();
        }

        /// <summary>
        /// Finds the shortest path between two people.
        /// </summary>
        /// <param name="request">The pathfinding request.</param>
        /// <returns>The path result.</returns>
        public object Get(FindPathRequest request)
        {
            this.logger.Info($"FindPath endpoint called: {request.FromPersonId} -> {request.ToPersonId}");
            return this.pathfindingService.FindShortestPath(request.FromPersonId, request.ToPersonId, request.MaxDepth);
        }

        /// <summary>
        /// Gets N-degree neighbors for a person.
        /// </summary>
        /// <param name="request">The neighbors request.</param>
        /// <returns>The neighbors result.</returns>
        public object Get(GetNeighborsRequest request)
        {
            this.logger.Info($"GetNeighbors endpoint called: {request.PersonId}, Degree: {request.Degree}, MaxNodes: {request.MaxNodes}");
            return this.pathfindingService.GetNeighbors(request.PersonId, request.Degree, request.MaxNodes);
        }

        /// <summary>
        /// Searches for people by name.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>Search results with pagination.</returns>
        public object Get(SearchPeopleRequest request)
        {
            this.logger.Info($"SearchPeople endpoint called: Query='{request.Query}', Limit={request.Limit}, Offset={request.Offset}");

            var results = this.graph.SearchPeople(request.Query, request.Limit, request.Offset);
            var resultList = results.ToList();

            return new
            {
                Query = request.Query,
                Limit = request.Limit,
                Offset = request.Offset,
                Count = resultList.Count,
                Results = resultList.Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    ConnectionCount = p.MediaConnections.Count
                })
            };
        }

        /// <summary>
        /// Gets all people with pagination.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Paginated list of people.</returns>
        public object Get(GetPeopleRequest request)
        {
            this.logger.Info($"GetPeople endpoint called: Limit={request.Limit}, Offset={request.Offset}");

            var results = this.graph.GetAllPeople(request.Limit, request.Offset);
            var resultList = results.ToList();

            return new
            {
                Limit = request.Limit,
                Offset = request.Offset,
                Count = resultList.Count,
                TotalCount = this.graph.PeopleCount,
                Results = resultList.Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    ConnectionCount = p.MediaConnections.Count
                })
            };
        }

        /// <summary>
        /// Triggers a graph rebuild.
        /// </summary>
        /// <param name="request">The rebuild request.</param>
        /// <returns>The rebuild status.</returns>
        public object Post(RebuildGraphRequest request)
        {
            this.logger.Info("RebuildGraph endpoint called");

            // TODO: Implement graph rebuild logic
            return new
            {
                Success = false,
                Message = "Graph rebuild not yet implemented"
            };
        }
    }

    /// <summary>
    /// Request for getting graph statistics.
    /// </summary>
    [Route("/SixDegrees/Statistics", "GET")]
    public class GetStatisticsRequest : IReturn<object>
    {
    }

    /// <summary>
    /// Request for finding path between two people.
    /// </summary>
    [Route("/SixDegrees/FindPath", "GET")]
    public class FindPathRequest : IReturn<object>
    {
        /// <summary>
        /// Gets or sets the source person ID.
        /// </summary>
        public string FromPersonId { get; set; }

        /// <summary>
        /// Gets or sets the target person ID.
        /// </summary>
        public string ToPersonId { get; set; }

        /// <summary>
        /// Gets or sets the maximum search depth (default: 6).
        /// </summary>
        public int MaxDepth { get; set; } = 6;
    }

    /// <summary>
    /// Request for getting N-degree neighbors.
    /// </summary>
    [Route("/SixDegrees/Neighbors", "GET")]
    public class GetNeighborsRequest : IReturn<object>
    {
        /// <summary>
        /// Gets or sets the person ID.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the degree (1-6).
        /// </summary>
        public int Degree { get; set; } = 2;

        /// <summary>
        /// Gets or sets the maximum number of nodes to return (default: 500, max: 1000).
        /// </summary>
        public int MaxNodes { get; set; } = 500;
    }

    /// <summary>
    /// Request for searching people.
    /// </summary>
    [Route("/SixDegrees/Search", "GET")]
    public class SearchPeopleRequest : IReturn<object>
    {
        /// <summary>
        /// Gets or sets the search query.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results (default: 20, max: 100).
        /// </summary>
        public int Limit { get; set; } = 20;

        /// <summary>
        /// Gets or sets the number of results to skip (default: 0).
        /// </summary>
        public int Offset { get; set; } = 0;
    }

    /// <summary>
    /// Request for getting all people.
    /// </summary>
    [Route("/SixDegrees/People", "GET")]
    public class GetPeopleRequest : IReturn<object>
    {
        /// <summary>
        /// Gets or sets the maximum number of results (default: 50, max: 200).
        /// </summary>
        public int Limit { get; set; } = 50;

        /// <summary>
        /// Gets or sets the number of results to skip (default: 0).
        /// </summary>
        public int Offset { get; set; } = 0;
    }

    /// <summary>
    /// Request for rebuilding the graph.
    /// </summary>
    [Route("/SixDegrees/Rebuild", "POST")]
    public class RebuildGraphRequest : IReturn<object>
    {
    }
}
