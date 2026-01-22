// <copyright file="PathfindingServiceTests.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Tests;

using SixDegrees.Models;
using SixDegrees.Services;
using Xunit;

/// <summary>
/// Unit tests for the PathfindingService.
/// </summary>
public class PathfindingServiceTests
{
    private readonly TestLogger logger;
    private readonly RelationshipGraph graph;
    private readonly PathfindingService service;

    public PathfindingServiceTests()
    {
        this.logger = new TestLogger();
        this.graph = new RelationshipGraph(this.logger);
        this.service = new PathfindingService(this.logger, this.graph);
    }

    /// <summary>
    /// Creates a test graph with known connections:
    /// Tom Hanks → Forrest Gump → Gary Sinise
    /// Tom Hanks → Cast Away → (only Tom)
    /// Gary Sinise → Apollo 13 → Kevin Bacon
    /// Kevin Bacon → A Few Good Men → Tom Cruise
    /// Isolated Person (no connections).
    /// </summary>
    private void SetupTestGraph()
    {
        // Add people
        this.graph.AddPerson(new PersonNode { Id = "tom-hanks", Name = "Tom Hanks" });
        this.graph.AddPerson(new PersonNode { Id = "gary-sinise", Name = "Gary Sinise" });
        this.graph.AddPerson(new PersonNode { Id = "kevin-bacon", Name = "Kevin Bacon" });
        this.graph.AddPerson(new PersonNode { Id = "tom-cruise", Name = "Tom Cruise" });
        this.graph.AddPerson(new PersonNode { Id = "isolated-person", Name = "Isolated Person" });

        // Add media
        this.graph.AddMedia(new MediaNode { Id = "forrest-gump", Name = "Forrest Gump", MediaType = "Movie", Year = 1994 });
        this.graph.AddMedia(new MediaNode { Id = "cast-away", Name = "Cast Away", MediaType = "Movie", Year = 2000 });
        this.graph.AddMedia(new MediaNode { Id = "apollo-13", Name = "Apollo 13", MediaType = "Movie", Year = 1995 });
        this.graph.AddMedia(new MediaNode { Id = "a-few-good-men", Name = "A Few Good Men", MediaType = "Movie", Year = 1992 });

        // Create connections
        // Tom Hanks and Gary Sinise in Forrest Gump
        this.graph.AddConnection("tom-hanks", "forrest-gump", "Actor");
        this.graph.AddConnection("gary-sinise", "forrest-gump", "Actor");

        // Tom Hanks alone in Cast Away
        this.graph.AddConnection("tom-hanks", "cast-away", "Actor");

        // Gary Sinise and Kevin Bacon in Apollo 13
        this.graph.AddConnection("gary-sinise", "apollo-13", "Actor");
        this.graph.AddConnection("kevin-bacon", "apollo-13", "Actor");

        // Kevin Bacon and Tom Cruise in A Few Good Men
        this.graph.AddConnection("kevin-bacon", "a-few-good-men", "Actor");
        this.graph.AddConnection("tom-cruise", "a-few-good-men", "Actor");

        // Isolated person has no connections
    }

    #region FindShortestPath Tests

    [Fact]
    public void FindShortestPath_DirectConnection_ReturnsTwoDegrees()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "gary-sinise");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Degrees);
        Assert.NotNull(result.Path);
        Assert.Equal(3, result.Path.Count); // Tom Hanks -> Forrest Gump -> Gary Sinise

        // Verify path structure
        Assert.Equal("person", result.Path[0].Type);
        Assert.Equal("Tom Hanks", result.Path[0].Name);
        Assert.Equal("media", result.Path[1].Type);
        Assert.Equal("Forrest Gump", result.Path[1].Name);
        Assert.Equal("person", result.Path[2].Type);
        Assert.Equal("Gary Sinise", result.Path[2].Name);
    }

    [Fact]
    public void FindShortestPath_IndirectConnection_ReturnsFourDegrees()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        // Tom Hanks -> Forrest Gump -> Gary Sinise -> Apollo 13 -> Kevin Bacon
        var result = this.service.FindShortestPath("tom-hanks", "kevin-bacon");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(4, result.Degrees);
        Assert.NotNull(result.Path);
        Assert.Equal(5, result.Path.Count); // 3 people + 2 media

        // Verify alternating pattern
        Assert.Equal("person", result.Path[0].Type);
        Assert.Equal("media", result.Path[1].Type);
        Assert.Equal("person", result.Path[2].Type);
        Assert.Equal("media", result.Path[3].Type);
        Assert.Equal("person", result.Path[4].Type);
    }

    [Fact]
    public void FindShortestPath_SixDegrees_ReturnsSixDegrees()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        // Tom Hanks -> FG -> Gary -> Apollo -> Kevin -> AFGM -> Tom Cruise
        var result = this.service.FindShortestPath("tom-hanks", "tom-cruise");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(6, result.Degrees);
        Assert.NotNull(result.Path);
        Assert.Equal(7, result.Path.Count); // 4 people + 3 media
    }

    [Fact]
    public void FindShortestPath_NoConnection_ReturnsNotFound()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "isolated-person");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.Path);
        Assert.Contains("No path found", result.Message);
    }

    [Fact]
    public void FindShortestPath_SamePerson_ReturnsError()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "tom-hanks");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("same", result.Message.ToLower());
    }

    [Fact]
    public void FindShortestPath_StartPersonNotFound_ReturnsError()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("nonexistent", "tom-hanks");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Start person not found", result.Message);
    }

    [Fact]
    public void FindShortestPath_TargetPersonNotFound_ReturnsError()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "nonexistent");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Target person not found", result.Message);
    }

    [Theory]
    [InlineData(null, "tom-hanks")]
    [InlineData("tom-hanks", null)]
    [InlineData("", "tom-hanks")]
    [InlineData("tom-hanks", "")]
    public void FindShortestPath_InvalidInput_ReturnsError(string? startId, string? targetId)
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath(startId!, targetId!);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.Message);
    }

    [Fact]
    public void FindShortestPath_ReturnsSearchTimeMetric()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "gary-sinise");

        // Assert
        Assert.True(result.SearchTimeMs >= 0);
    }

    [Fact]
    public void FindShortestPath_ReturnsNodesVisitedMetric()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "gary-sinise");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.NodesVisited > 0);
    }

    [Fact]
    public void FindShortestPath_NoPath_StillReturnsNodesVisitedMetric()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "isolated-person");

        // Assert
        Assert.False(result.Success);
        Assert.True(result.NodesVisited > 0); // Should have visited nodes during search
    }

    [Fact]
    public void FindShortestPath_MaxDepthRespected()
    {
        // Arrange
        this.SetupTestGraph();

        // Act - Try to find path with max depth of 1 (only allows 2-degree paths, should fail for 4-degree path)
        // Tom -> Kevin requires 2 person hops (4 degrees), but maxDepth=1 only allows 1 hop (2 degrees)
        var result = this.service.FindShortestPath("tom-hanks", "kevin-bacon", maxDepth: 1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No path found", result.Message);
    }

    [Fact]
    public void FindShortestPath_PathIncludesMediaType()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "gary-sinise");

        // Assert
        Assert.True(result.Success);
        var mediaNode = result.Path!.First(n => n.Type == "media");
        Assert.Equal("Movie", mediaNode.MediaType);
    }

    [Fact]
    public void FindShortestPath_PathIncludesRole()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.FindShortestPath("tom-hanks", "gary-sinise");

        // Assert
        Assert.True(result.Success);
        var mediaNode = result.Path!.First(n => n.Type == "media");
        Assert.NotNull(mediaNode.Role);
        Assert.Contains("Actor", mediaNode.Role);
    }

    #endregion

    #region BFS Algorithm Correctness Tests

    [Fact]
    public void FindShortestPath_FindsOptimalPath_WhenMultiplePathsExist()
    {
        // Arrange - Create a graph with two paths between A and D
        // Short path: A -> M1 -> D (2 degrees)
        // Long path: A -> M2 -> B -> M3 -> C -> M4 -> D (6 degrees)
        this.graph.AddPerson(new PersonNode { Id = "person-a", Name = "Person A" });
        this.graph.AddPerson(new PersonNode { Id = "person-b", Name = "Person B" });
        this.graph.AddPerson(new PersonNode { Id = "person-c", Name = "Person C" });
        this.graph.AddPerson(new PersonNode { Id = "person-d", Name = "Person D" });

        this.graph.AddMedia(new MediaNode { Id = "media-1", Name = "Media 1", MediaType = "Movie" });
        this.graph.AddMedia(new MediaNode { Id = "media-2", Name = "Media 2", MediaType = "Movie" });
        this.graph.AddMedia(new MediaNode { Id = "media-3", Name = "Media 3", MediaType = "Movie" });
        this.graph.AddMedia(new MediaNode { Id = "media-4", Name = "Media 4", MediaType = "Movie" });

        // Short path
        this.graph.AddConnection("person-a", "media-1", "Actor");
        this.graph.AddConnection("person-d", "media-1", "Actor");

        // Long path
        this.graph.AddConnection("person-a", "media-2", "Actor");
        this.graph.AddConnection("person-b", "media-2", "Actor");
        this.graph.AddConnection("person-b", "media-3", "Actor");
        this.graph.AddConnection("person-c", "media-3", "Actor");
        this.graph.AddConnection("person-c", "media-4", "Actor");
        this.graph.AddConnection("person-d", "media-4", "Actor");

        // Act
        var result = this.service.FindShortestPath("person-a", "person-d");

        // Assert - BFS should find the shortest path (2 degrees via Media 1)
        Assert.True(result.Success);
        Assert.Equal(2, result.Degrees);
        Assert.Equal(3, result.Path!.Count); // A -> M1 -> D
    }

    [Fact]
    public void FindShortestPath_HandlesLargeGraph()
    {
        // Arrange - Create a larger graph
        const int numPeople = 100;
        const int numMedia = 50;

        // Add people
        for (int i = 0; i < numPeople; i++)
        {
            this.graph.AddPerson(new PersonNode { Id = $"person-{i}", Name = $"Person {i}" });
        }

        // Add media
        for (int i = 0; i < numMedia; i++)
        {
            this.graph.AddMedia(new MediaNode { Id = $"media-{i}", Name = $"Media {i}", MediaType = "Movie" });
        }

        // Create linear chain: person-0 -> media-0 -> person-1 -> media-1 -> ... -> person-5
        for (int i = 0; i < 5; i++)
        {
            this.graph.AddConnection($"person-{i}", $"media-{i}", "Actor");
            this.graph.AddConnection($"person-{i + 1}", $"media-{i}", "Actor");
        }

        // Act
        var result = this.service.FindShortestPath("person-0", "person-5");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10, result.Degrees); // 5 steps * 2 degrees each
        Assert.True(result.SearchTimeMs < 1000); // Should complete within 1 second
    }

    #endregion
}
