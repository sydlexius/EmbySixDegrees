// <copyright file="NeighborsServiceTests.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Tests;

using SixDegrees.Models;
using SixDegrees.Services;
using Xunit;

/// <summary>
/// Unit tests for the GetNeighbors functionality of PathfindingService.
/// </summary>
public class NeighborsServiceTests
{
    private readonly TestLogger logger;
    private readonly RelationshipGraph graph;
    private readonly PathfindingService service;

    public NeighborsServiceTests()
    {
        this.logger = new TestLogger();
        this.graph = new RelationshipGraph(this.logger);
        this.service = new PathfindingService(this.logger, this.graph);
    }

    /// <summary>
    /// Creates a test graph with known connections.
    /// </summary>
    private void SetupTestGraph()
    {
        // Add people
        this.graph.AddPerson(new PersonNode { Id = "person-center", Name = "Center Person" });
        this.graph.AddPerson(new PersonNode { Id = "person-1", Name = "Person 1" });
        this.graph.AddPerson(new PersonNode { Id = "person-2", Name = "Person 2" });
        this.graph.AddPerson(new PersonNode { Id = "person-3", Name = "Person 3" });
        this.graph.AddPerson(new PersonNode { Id = "person-4", Name = "Person 4" });

        // Add media
        this.graph.AddMedia(new MediaNode { Id = "media-a", Name = "Media A", MediaType = "Movie" });
        this.graph.AddMedia(new MediaNode { Id = "media-b", Name = "Media B", MediaType = "Movie" });
        this.graph.AddMedia(new MediaNode { Id = "media-c", Name = "Media C", MediaType = "Series" });

        // Create connections
        // Center -> Media A -> Person 1
        this.graph.AddConnection("person-center", "media-a", "Actor");
        this.graph.AddConnection("person-1", "media-a", "Actor");

        // Center -> Media B -> Person 2, Person 3
        this.graph.AddConnection("person-center", "media-b", "Director");
        this.graph.AddConnection("person-2", "media-b", "Actor");
        this.graph.AddConnection("person-3", "media-b", "Actor");

        // Person 1 -> Media C -> Person 4 (second degree from center)
        this.graph.AddConnection("person-1", "media-c", "Actor");
        this.graph.AddConnection("person-4", "media-c", "Actor");
    }

    #region GetNeighbors Tests

    [Fact]
    public void GetNeighbors_DegreeOne_ReturnsDirectConnections()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 1);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Nodes);
        Assert.NotNull(result.Edges);

        // Should include center person, media-a, media-b, person-1, person-2, person-3
        var nodeNames = result.Nodes.Select(n => n.Name).ToList();
        Assert.Contains("Center Person", nodeNames);
        Assert.Contains("Media A", nodeNames);
        Assert.Contains("Media B", nodeNames);
        Assert.Contains("Person 1", nodeNames);
        Assert.Contains("Person 2", nodeNames);
        Assert.Contains("Person 3", nodeNames);

        // Should NOT include Person 4 (second degree)
        Assert.DoesNotContain("Person 4", nodeNames);
    }

    [Fact]
    public void GetNeighbors_DegreeTwo_ReturnsSecondDegreeConnections()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 2);

        // Assert
        Assert.True(result.Success);

        var nodeNames = result.Nodes!.Select(n => n.Name).ToList();

        // Should include second degree connections now
        Assert.Contains("Person 4", nodeNames);
        Assert.Contains("Media C", nodeNames);
    }

    [Fact]
    public void GetNeighbors_InvalidPersonId_ReturnsError()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("nonexistent", degree: 1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message.ToLower());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetNeighbors_EmptyPersonId_ReturnsError(string? personId)
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors(personId!, degree: 1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.Message);
    }

    [Fact]
    public void GetNeighbors_ReturnsSearchTimeMetric()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 1);

        // Assert
        Assert.True(result.SearchTimeMs >= 0);
    }

    [Fact]
    public void GetNeighbors_ReturnsNodesVisitedMetric()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 1);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.NodesVisited > 0);
    }

    [Fact]
    public void GetNeighbors_DegreeClamped_WhenTooHigh()
    {
        // Arrange
        this.SetupTestGraph();

        // Act - Request degree 10, should be clamped to 6
        var result = this.service.GetNeighbors("person-center", degree: 10);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(6, result.Degree); // Should be clamped to 6
    }

    [Fact]
    public void GetNeighbors_DegreeClamped_WhenTooLow()
    {
        // Arrange
        this.SetupTestGraph();

        // Act - Request degree 0, should be clamped to 1
        var result = this.service.GetNeighbors("person-center", degree: 0);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.Degree); // Should be clamped to 1
    }

    [Fact]
    public void GetNeighbors_MaxNodes_TruncatesResults()
    {
        // Arrange - Create a larger graph
        this.graph.AddPerson(new PersonNode { Id = "center", Name = "Center" });

        for (int i = 0; i < 20; i++)
        {
            this.graph.AddPerson(new PersonNode { Id = $"p{i}", Name = $"Person {i}" });
            this.graph.AddMedia(new MediaNode { Id = $"m{i}", Name = $"Media {i}", MediaType = "Movie" });
            this.graph.AddConnection("center", $"m{i}", "Actor");
            this.graph.AddConnection($"p{i}", $"m{i}", "Actor");
        }

        // Act - Request with low maxNodes
        var result = this.service.GetNeighbors("center", degree: 1, maxNodes: 5);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Truncated);
        Assert.True(result.Nodes!.Count <= 5);
    }

    [Fact]
    public void GetNeighbors_EdgesConnectCorrectly()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 1);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.Edges!);

        // Verify all edges reference valid nodes
        var nodeIds = result.Nodes!.Select(n => n.Id).ToHashSet();
        foreach (var edge in result.Edges)
        {
            Assert.Contains(edge.Source, nodeIds);
            Assert.Contains(edge.Target, nodeIds);
        }
    }

    [Fact]
    public void GetNeighbors_IncludesRoleInEdges()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 1);

        // Assert
        Assert.True(result.Success);

        // Find edge from center to media-b (where center is Director)
        var directorEdge = result.Edges!.FirstOrDefault(e =>
            e.Source == "person-center" && e.Target == "media-b");

        Assert.NotNull(directorEdge);
        Assert.Equal("Director", directorEdge!.Role);
    }

    [Fact]
    public void GetNeighbors_NodesHaveCorrectTypes()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 1);

        // Assert
        Assert.True(result.Success);

        foreach (var node in result.Nodes!)
        {
            Assert.True(node.Type == "person" || node.Type == "media");

            if (node.Type == "media")
            {
                Assert.NotNull(node.MediaType);
            }
        }
    }

    [Fact]
    public void GetNeighbors_PersonWithNoConnections_ReturnsOnlySelf()
    {
        // Arrange
        this.graph.AddPerson(new PersonNode { Id = "lonely", Name = "Lonely Person" });

        // Act
        var result = this.service.GetNeighbors("lonely", degree: 1);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Nodes!);
        Assert.Equal("Lonely Person", result.Nodes[0].Name);
        Assert.Empty(result.Edges!);
    }

    [Fact]
    public void GetNeighbors_NodesHaveCorrectDepth()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 2);

        // Assert
        Assert.True(result.Success);

        // Center person should have depth 0
        var centerNode = result.Nodes!.First(n => n.Id == "person-center");
        Assert.Equal(0, centerNode.Depth);

        // First degree people (Person 1, 2, 3) should have depth 1
        var person1 = result.Nodes!.First(n => n.Id == "person-1");
        Assert.Equal(1, person1.Depth);

        var person2 = result.Nodes!.First(n => n.Id == "person-2");
        Assert.Equal(1, person2.Depth);

        // Media connected to center should have depth 0
        var mediaA = result.Nodes!.First(n => n.Id == "media-a");
        Assert.Equal(0, mediaA.Depth);

        // Second degree person (Person 4) should have depth 2
        var person4 = result.Nodes!.First(n => n.Id == "person-4");
        Assert.Equal(2, person4.Depth);

        // Media C (connected through Person 1) should have depth 1
        var mediaC = result.Nodes!.First(n => n.Id == "media-c");
        Assert.Equal(1, mediaC.Depth);
    }

    [Fact]
    public void GetNeighbors_DepthZero_ForStartPerson()
    {
        // Arrange
        this.SetupTestGraph();

        // Act
        var result = this.service.GetNeighbors("person-center", degree: 1);

        // Assert
        Assert.True(result.Success);

        var startNode = result.Nodes!.First(n => n.Id == "person-center");
        Assert.NotNull(startNode.Depth);
        Assert.Equal(0, startNode.Depth);
    }

    #endregion
}
