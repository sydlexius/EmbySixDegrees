// <copyright file="PathfindingService.cs" company="Six Degrees">
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
    /// Service for finding paths between people in the relationship graph.
    /// </summary>
    public class PathfindingService
    {
        private readonly ILogger logger;
        private readonly RelationshipGraph graph;
        private const int MaxSearchDepth = 6; // Six degrees of separation

        /// <summary>
        /// Initializes a new instance of the <see cref="PathfindingService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="graph">The relationship graph.</param>
        public PathfindingService(ILogger logger, RelationshipGraph graph)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        /// <summary>
        /// Finds the shortest path between two people using BFS.
        /// </summary>
        /// <param name="startPersonId">The starting person ID.</param>
        /// <param name="targetPersonId">The target person ID.</param>
        /// <param name="maxDepth">Maximum search depth (default: 6).</param>
        /// <returns>A path result containing the path or null if no path found.</returns>
        public PathResult FindShortestPath(string startPersonId, string targetPersonId, int maxDepth = MaxSearchDepth)
        {
            if (string.IsNullOrEmpty(startPersonId) || string.IsNullOrEmpty(targetPersonId))
            {
                return new PathResult { Success = false, Message = "Invalid person IDs" };
            }

            if (startPersonId == targetPersonId)
            {
                return new PathResult { Success = false, Message = "Start and target person are the same" };
            }

            var startPerson = this.graph.GetPerson(startPersonId);
            var targetPerson = this.graph.GetPerson(targetPersonId);

            if (startPerson == null)
            {
                return new PathResult { Success = false, Message = "Start person not found" };
            }

            if (targetPerson == null)
            {
                return new PathResult { Success = false, Message = "Target person not found" };
            }

            var startTime = DateTime.UtcNow;

            try
            {
                var (path, nodesVisited) = this.BreadthFirstSearch(startPersonId, targetPersonId, maxDepth);

                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                this.logger.Info($"Path search completed in {elapsedMs}ms, visited {nodesVisited} nodes");

                if (path == null)
                {
                    return new PathResult
                    {
                        Success = false,
                        Message = $"No path found within {maxDepth} degrees",
                        SearchTimeMs = elapsedMs,
                        NodesVisited = nodesVisited
                    };
                }

                return new PathResult
                {
                    Success = true,
                    Path = path,
                    Degrees = path.Count - 1, // Number of edges in the path (Person->Media->Person = 2 degrees)
                    SearchTimeMs = elapsedMs,
                    NodesVisited = nodesVisited
                };
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error finding path: {ex.Message}");
                return new PathResult
                {
                    Success = false,
                    Message = $"Error during search: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Performs breadth-first search to find the shortest path.
        /// </summary>
        /// <param name="startPersonId">The starting person ID.</param>
        /// <param name="targetPersonId">The target person ID.</param>
        /// <param name="maxDepth">Maximum search depth.</param>
        /// <returns>A tuple containing the path nodes (or null if no path found) and the number of nodes visited.</returns>
        private (List<PathNode> Path, int NodesVisited) BreadthFirstSearch(string startPersonId, string targetPersonId, int maxDepth)
        {
            var queue = new Queue<(string PersonId, int Depth)>();
            var visited = new HashSet<string>();
            var parent = new Dictionary<string, PathEdge>();

            queue.Enqueue((startPersonId, 0));
            visited.Add(startPersonId);

            while (queue.Count > 0)
            {
                var (currentPersonId, depth) = queue.Dequeue();

                // Check if we've reached the target
                if (currentPersonId == targetPersonId)
                {
                    return (this.ReconstructPath(parent, startPersonId, targetPersonId), visited.Count);
                }

                // Stop if we've exceeded max depth
                if (depth >= maxDepth)
                {
                    continue;
                }

                var currentPerson = this.graph.GetPerson(currentPersonId);
                if (currentPerson == null)
                {
                    continue;
                }

                // Explore all media connections
                foreach (var mediaConnection in currentPerson.MediaConnections.Values)
                {
                    var mediaItem = this.graph.GetMedia(mediaConnection.MediaId);
                    if (mediaItem == null)
                    {
                        continue;
                    }

                    // Explore all people connected through this media
                    foreach (var personConnection in mediaItem.PeopleConnections.Values)
                    {
                        var nextPersonId = personConnection.PersonId;

                        if (!visited.Contains(nextPersonId))
                        {
                            visited.Add(nextPersonId);
                            parent[nextPersonId] = new PathEdge
                            {
                                FromPersonId = currentPersonId,
                                MediaId = mediaConnection.MediaId,
                                MediaName = mediaConnection.MediaName,
                                MediaType = mediaConnection.MediaType,
                                FromRole = mediaConnection.Role,
                                ToRole = personConnection.Role
                            };
                            queue.Enqueue((nextPersonId, depth + 1));
                        }
                    }
                }
            }

            return (null, visited.Count); // No path found
        }

        /// <summary>
        /// Reconstructs the path from parent dictionary.
        /// </summary>
        /// <param name="parent">The parent dictionary.</param>
        /// <param name="startPersonId">The starting person ID.</param>
        /// <param name="targetPersonId">The target person ID.</param>
        /// <returns>A list of path nodes.</returns>
        private List<PathNode> ReconstructPath(Dictionary<string, PathEdge> parent, string startPersonId, string targetPersonId)
        {
            var path = new List<PathNode>();
            var currentPersonId = targetPersonId;

            // Build path backwards from target to start
            var tempPath = new List<(string PersonId, PathEdge Edge)>();
            while (currentPersonId != startPersonId)
            {
                var edge = parent[currentPersonId];
                tempPath.Add((currentPersonId, edge));
                currentPersonId = edge.FromPersonId;
            }

            // Add start person
            var startPerson = this.graph.GetPerson(startPersonId);
            path.Add(new PathNode
            {
                Type = "person",
                Id = startPerson.Id,
                Name = startPerson.Name,
                ImageUrl = startPerson.ImageUrl
            });

            // Reverse and add to path
            tempPath.Reverse();
            foreach (var (personId, edge) in tempPath)
            {
                // Add media node
                path.Add(new PathNode
                {
                    Type = "media",
                    Id = edge.MediaId,
                    Name = edge.MediaName,
                    MediaType = edge.MediaType,
                    Role = $"{edge.FromRole} / {edge.ToRole}"
                });

                // Add person node
                var person = this.graph.GetPerson(personId);
                path.Add(new PathNode
                {
                    Type = "person",
                    Id = person.Id,
                    Name = person.Name,
                    ImageUrl = person.ImageUrl
                });
            }

            return path;
        }

        /// <summary>
        /// Gets N-degree neighbors for a person with node/edge limits.
        /// </summary>
        /// <param name="personId">The person ID.</param>
        /// <param name="degree">The degree of separation (1-6).</param>
        /// <param name="maxNodes">Maximum number of nodes to return (default: 500).</param>
        /// <returns>A neighbors result containing nodes and edges.</returns>
        public NeighborsResult GetNeighbors(string personId, int degree, int maxNodes = 500)
        {
            if (string.IsNullOrEmpty(personId))
            {
                return new NeighborsResult { Success = false, Message = "Invalid person ID" };
            }

            degree = Math.Max(1, Math.Min(degree, 6)); // Clamp between 1 and 6
            maxNodes = Math.Max(1, Math.Min(maxNodes, 1000)); // Clamp between 1 and 1000

            var person = this.graph.GetPerson(personId);
            if (person == null)
            {
                return new NeighborsResult { Success = false, Message = "Person not found" };
            }

            var startTime = DateTime.UtcNow;

            try
            {
                var (nodes, edges, nodesVisited) = this.BreadthFirstExpansion(personId, degree, maxNodes);

                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                this.logger.Info($"Neighbor expansion completed in {elapsedMs}ms: {nodes.Count} nodes, {edges.Count} edges, visited {nodesVisited} nodes");

                return new NeighborsResult
                {
                    Success = true,
                    PersonId = personId,
                    Degree = degree,
                    Nodes = nodes,
                    Edges = edges,
                    Truncated = nodes.Count >= maxNodes,
                    SearchTimeMs = elapsedMs,
                    NodesVisited = nodesVisited
                };
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error getting neighbors: {ex.Message}");
                return new NeighborsResult
                {
                    Success = false,
                    Message = $"Error during expansion: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Performs breadth-first expansion to get N-degree neighbors.
        /// </summary>
        /// <param name="startPersonId">The starting person ID.</param>
        /// <param name="maxDegree">Maximum degree of separation.</param>
        /// <param name="maxNodes">Maximum number of nodes to return.</param>
        /// <returns>A tuple containing nodes, edges, and nodes visited count.</returns>
        private (List<GraphNode> Nodes, List<GraphEdge> Edges, int NodesVisited) BreadthFirstExpansion(
            string startPersonId,
            int maxDegree,
            int maxNodes)
        {
            var nodes = new List<GraphNode>();
            var edges = new List<GraphEdge>();
            var visitedPeople = new HashSet<string>();
            var visitedMedia = new HashSet<string>();
            var personDepth = new Dictionary<string, int>();
            var mediaDepth = new Dictionary<string, int>();
            var queue = new Queue<(string PersonId, int Depth)>();

            queue.Enqueue((startPersonId, 0));
            visitedPeople.Add(startPersonId);
            personDepth[startPersonId] = 0;

            // Add start person
            var startPerson = this.graph.GetPerson(startPersonId);
            nodes.Add(new GraphNode
            {
                Id = startPerson.Id,
                Name = startPerson.Name,
                Type = "person",
                ImageUrl = startPerson.ImageUrl,
                Depth = 0
            });

            while (queue.Count > 0 && nodes.Count < maxNodes)
            {
                var (currentPersonId, depth) = queue.Dequeue();

                if (depth >= maxDegree)
                {
                    continue;
                }

                var currentPerson = this.graph.GetPerson(currentPersonId);
                if (currentPerson == null)
                {
                    continue;
                }

                foreach (var mediaConnection in currentPerson.MediaConnections.Values)
                {
                    if (nodes.Count >= maxNodes)
                    {
                        break;
                    }

                    // Add media node if not already visited
                    if (!visitedMedia.Contains(mediaConnection.MediaId))
                    {
                        visitedMedia.Add(mediaConnection.MediaId);
                        mediaDepth[mediaConnection.MediaId] = depth;
                        nodes.Add(new GraphNode
                        {
                            Id = mediaConnection.MediaId,
                            Name = mediaConnection.MediaName,
                            Type = "media",
                            MediaType = mediaConnection.MediaType,
                            ImageUrl = mediaConnection.ImageUrl,
                            Depth = depth
                        });
                    }

                    // Add edge from person to media
                    edges.Add(new GraphEdge
                    {
                        Source = currentPersonId,
                        Target = mediaConnection.MediaId,
                        Role = mediaConnection.Role
                    });

                    var mediaItem = this.graph.GetMedia(mediaConnection.MediaId);
                    if (mediaItem == null)
                    {
                        continue;
                    }

                    // Add connected people
                    foreach (var personConnection in mediaItem.PeopleConnections.Values)
                    {
                        if (nodes.Count >= maxNodes)
                        {
                            break;
                        }

                        var nextPersonId = personConnection.PersonId;

                        if (!visitedPeople.Contains(nextPersonId))
                        {
                            visitedPeople.Add(nextPersonId);
                            personDepth[nextPersonId] = depth + 1;

                            var nextPerson = this.graph.GetPerson(nextPersonId);
                            if (nextPerson != null)
                            {
                                nodes.Add(new GraphNode
                                {
                                    Id = nextPerson.Id,
                                    Name = nextPerson.Name,
                                    Type = "person",
                                    ImageUrl = nextPerson.ImageUrl,
                                    Depth = depth + 1
                                });

                                queue.Enqueue((nextPersonId, depth + 1));
                            }
                        }

                        // Add edge from media to person (only if both nodes exist)
                        if (visitedPeople.Contains(nextPersonId))
                        {
                            edges.Add(new GraphEdge
                            {
                                Source = mediaConnection.MediaId,
                                Target = nextPersonId,
                                Role = personConnection.Role
                            });
                        }
                    }
                }
            }

            return (nodes, edges, visitedPeople.Count + visitedMedia.Count);
        }
    }

    /// <summary>
    /// Represents an edge in the parent dictionary for path reconstruction.
    /// </summary>
    internal class PathEdge
    {
        public string FromPersonId { get; set; }

        public string MediaId { get; set; }

        public string MediaName { get; set; }

        public string MediaType { get; set; }

        public string FromRole { get; set; }

        public string ToRole { get; set; }
    }

    /// <summary>
    /// Result of a pathfinding operation.
    /// </summary>
    public class PathResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public List<PathNode> Path { get; set; }

        public int Degrees { get; set; }

        public double SearchTimeMs { get; set; }

        public int NodesVisited { get; set; }
    }

    /// <summary>
    /// Represents a node in a path.
    /// </summary>
    public class PathNode
    {
        public string Type { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string MediaType { get; set; }

        public string ImageUrl { get; set; }

        public string Role { get; set; }
    }

    /// <summary>
    /// Result of a neighbor expansion operation.
    /// </summary>
    public class NeighborsResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string PersonId { get; set; }

        public int Degree { get; set; }

        public List<GraphNode> Nodes { get; set; }

        public List<GraphEdge> Edges { get; set; }

        public bool Truncated { get; set; }

        public double SearchTimeMs { get; set; }

        public int NodesVisited { get; set; }
    }

    /// <summary>
    /// Represents an edge in the graph visualization.
    /// </summary>
    public class GraphEdge
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public string Role { get; set; }
    }
}
