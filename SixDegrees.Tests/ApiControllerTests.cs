// <copyright file="ApiControllerTests.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Tests;

using System.Linq;
using SixDegrees.Api;
using SixDegrees.Models;
using SixDegrees.Services;
using Xunit;

/// <summary>
/// Unit tests for the API request DTOs and response models.
/// These tests verify request validation, default values, and response structure.
/// </summary>
public class ApiControllerTests
{
    #region SearchPeople Request Tests

    [Fact]
    public void SearchPeople_DefaultLimit_IsTwenty()
    {
        // Arrange & Act
        var request = new SearchPeople();

        // Assert
        Assert.Equal(20, request.Limit);
    }

    [Fact]
    public void SearchPeople_QueryProperty_CanBeSet()
    {
        // Arrange
        var request = new SearchPeople { Query = "Tom Hanks" };

        // Assert
        Assert.Equal("Tom Hanks", request.Query);
    }

    [Fact]
    public void SearchPeople_LimitProperty_CanBeSet()
    {
        // Arrange
        var request = new SearchPeople { Limit = 50 };

        // Assert
        Assert.Equal(50, request.Limit);
    }

    #endregion

    #region GetGraph Request Tests

    [Fact]
    public void GetGraph_DefaultDepth_IsTwo()
    {
        // Arrange & Act
        var request = new GetGraph();

        // Assert
        Assert.Equal(2, request.Depth);
    }

    [Fact]
    public void GetGraph_PersonIdProperty_CanBeSet()
    {
        // Arrange
        var request = new GetGraph { PersonId = "person-123" };

        // Assert
        Assert.Equal("person-123", request.PersonId);
    }

    [Fact]
    public void GetGraph_DepthProperty_CanBeSet()
    {
        // Arrange
        var request = new GetGraph { Depth = 4 };

        // Assert
        Assert.Equal(4, request.Depth);
    }

    #endregion

    #region GetShortestPath Request Tests

    [Fact]
    public void GetShortestPath_FromPersonIdProperty_CanBeSet()
    {
        // Arrange
        var request = new GetShortestPath { FromPersonId = "person-a" };

        // Assert
        Assert.Equal("person-a", request.FromPersonId);
    }

    [Fact]
    public void GetShortestPath_ToPersonIdProperty_CanBeSet()
    {
        // Arrange
        var request = new GetShortestPath { ToPersonId = "person-b" };

        // Assert
        Assert.Equal("person-b", request.ToPersonId);
    }

    #endregion

    #region GetNeighbors Request Tests

    [Fact]
    public void GetNeighbors_DefaultDegree_IsTwo()
    {
        // Arrange & Act
        var request = new GetNeighbors();

        // Assert
        Assert.Equal(2, request.Degree);
    }

    [Fact]
    public void GetNeighbors_DefaultMaxNodes_IsFiveHundred()
    {
        // Arrange & Act
        var request = new GetNeighbors();

        // Assert
        Assert.Equal(500, request.MaxNodes);
    }

    [Fact]
    public void GetNeighbors_PersonIdProperty_CanBeSet()
    {
        // Arrange
        var request = new GetNeighbors { PersonId = "person-123" };

        // Assert
        Assert.Equal("person-123", request.PersonId);
    }

    #endregion

    #region GetPeople Request Tests

    [Fact]
    public void GetPeople_DefaultLimit_IsFifty()
    {
        // Arrange & Act
        var request = new GetPeople();

        // Assert
        Assert.Equal(50, request.Limit);
    }

    [Fact]
    public void GetPeople_DefaultOffset_IsZero()
    {
        // Arrange & Act
        var request = new GetPeople();

        // Assert
        Assert.Equal(0, request.Offset);
    }

    #endregion

    #region Response DTO Tests

    [Fact]
    public void PersonSearchResult_AllPropertiesCanBeSet()
    {
        // Arrange & Act
        var result = new PersonSearchResult
        {
            Id = "person-123",
            Name = "Tom Hanks",
            ImageUrl = "/emby/Items/person-123/Images/Primary",
            ConnectionCount = 42
        };

        // Assert
        Assert.Equal("person-123", result.Id);
        Assert.Equal("Tom Hanks", result.Name);
        Assert.Equal("/emby/Items/person-123/Images/Primary", result.ImageUrl);
        Assert.Equal(42, result.ConnectionCount);
    }

    [Fact]
    public void PersonSearchResponse_AllPropertiesCanBeSet()
    {
        // Arrange & Act
        var response = new PersonSearchResponse
        {
            Query = "Tom",
            Limit = 20,
            Offset = 0,
            Count = 2,
            Results = new System.Collections.Generic.List<PersonSearchResult>
            {
                new PersonSearchResult { Id = "1", Name = "Tom Hanks" },
                new PersonSearchResult { Id = "2", Name = "Tom Cruise" }
            }
        };

        // Assert
        Assert.Equal("Tom", response.Query);
        Assert.Equal(20, response.Limit);
        Assert.Equal(0, response.Offset);
        Assert.Equal(2, response.Count);
        Assert.Equal(2, response.Results.Count);
    }

    [Fact]
    public void GraphData_SuccessResponse_HasAllFields()
    {
        // Arrange & Act
        var response = new GraphData
        {
            Success = true,
            Message = null,
            PersonId = "person-123",
            Depth = 2,
            Nodes = new System.Collections.Generic.List<GraphNodeDto>
            {
                new GraphNodeDto { Id = "person-123", Name = "Tom Hanks", Type = "person" }
            },
            Edges = new System.Collections.Generic.List<GraphEdgeDto>(),
            Truncated = false,
            SearchTimeMs = 12.5,
            NodesVisited = 10
        };

        // Assert
        Assert.True(response.Success);
        Assert.Null(response.Message);
        Assert.Equal("person-123", response.PersonId);
        Assert.Equal(2, response.Depth);
        Assert.Single(response.Nodes);
        Assert.Empty(response.Edges);
        Assert.False(response.Truncated);
        Assert.Equal(12.5, response.SearchTimeMs);
        Assert.Equal(10, response.NodesVisited);
    }

    [Fact]
    public void GraphData_ErrorResponse_HasMessage()
    {
        // Arrange & Act
        var response = new GraphData
        {
            Success = false,
            Message = "PersonId is required"
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("PersonId is required", response.Message);
    }

    [Fact]
    public void PathResultDto_SuccessResponse_HasPath()
    {
        // Arrange & Act
        var response = new PathResultDto
        {
            Success = true,
            Path = new System.Collections.Generic.List<PathNodeDto>
            {
                new PathNodeDto { Type = "person", Id = "1", Name = "Person A" },
                new PathNodeDto { Type = "media", Id = "m1", Name = "Movie 1", MediaType = "Movie", Role = "Actor / Actor" },
                new PathNodeDto { Type = "person", Id = "2", Name = "Person B" }
            },
            Degrees = 2,
            SearchTimeMs = 5.0,
            NodesVisited = 15
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal(3, response.Path.Count);
        Assert.Equal(2, response.Degrees);
    }

    [Fact]
    public void PathResultDto_ErrorResponse_HasMessage()
    {
        // Arrange & Act
        var response = new PathResultDto
        {
            Success = false,
            Message = "FromPersonId is required"
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("FromPersonId is required", response.Message);
        Assert.Null(response.Path);
    }

    [Fact]
    public void CacheStatusResult_SuccessResponse_HasCounts()
    {
        // Arrange & Act
        var response = new CacheStatusResult
        {
            Success = true,
            Message = "Cache rebuilt successfully",
            PeopleCount = 1000,
            MediaCount = 500,
            ConnectionCount = 5000,
            BuildTimeMs = 1500.5
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1000, response.PeopleCount);
        Assert.Equal(500, response.MediaCount);
        Assert.Equal(5000, response.ConnectionCount);
        Assert.Equal(1500.5, response.BuildTimeMs);
    }

    [Fact]
    public void StatisticsResponse_AllPropertiesCanBeSet()
    {
        // Arrange & Act
        var response = new StatisticsResponse
        {
            PeopleCount = 1000,
            MediaCount = 500,
            ConnectionCount = 5000,
            LastBuildTime = "2026-01-21T12:00:00Z"
        };

        // Assert
        Assert.Equal(1000, response.PeopleCount);
        Assert.Equal(500, response.MediaCount);
        Assert.Equal(5000, response.ConnectionCount);
        Assert.Equal("2026-01-21T12:00:00Z", response.LastBuildTime);
    }

    [Fact]
    public void GraphNodeDto_PersonNode_HasCorrectType()
    {
        // Arrange & Act
        var node = new GraphNodeDto
        {
            Id = "person-123",
            Name = "Tom Hanks",
            Type = "person",
            ImageUrl = "/emby/Items/person-123/Images/Primary",
            Depth = 0
        };

        // Assert
        Assert.Equal("person", node.Type);
        Assert.Null(node.MediaType);
    }

    [Fact]
    public void GraphNodeDto_MediaNode_HasMediaType()
    {
        // Arrange & Act
        var node = new GraphNodeDto
        {
            Id = "media-123",
            Name = "Forrest Gump",
            Type = "media",
            MediaType = "Movie",
            ImageUrl = "/emby/Items/media-123/Images/Primary",
            Depth = 1
        };

        // Assert
        Assert.Equal("media", node.Type);
        Assert.Equal("Movie", node.MediaType);
    }

    [Fact]
    public void GraphEdgeDto_AllPropertiesCanBeSet()
    {
        // Arrange & Act
        var edge = new GraphEdgeDto
        {
            Source = "person-123",
            Target = "media-456",
            Role = "Actor"
        };

        // Assert
        Assert.Equal("person-123", edge.Source);
        Assert.Equal("media-456", edge.Target);
        Assert.Equal("Actor", edge.Role);
    }

    [Fact]
    public void PathNodeDto_AllPropertiesCanBeSet()
    {
        // Arrange & Act
        var node = new PathNodeDto
        {
            Type = "media",
            Id = "media-123",
            Name = "Forrest Gump",
            MediaType = "Movie",
            ImageUrl = "/emby/Items/media-123/Images/Primary",
            Role = "Actor / Director"
        };

        // Assert
        Assert.Equal("media", node.Type);
        Assert.Equal("media-123", node.Id);
        Assert.Equal("Forrest Gump", node.Name);
        Assert.Equal("Movie", node.MediaType);
        Assert.Equal("Actor / Director", node.Role);
    }

    #endregion

    #region PeopleListResponse Tests

    [Fact]
    public void PeopleListResponse_AllPropertiesCanBeSet()
    {
        // Arrange & Act
        var response = new PeopleListResponse
        {
            Limit = 50,
            Offset = 100,
            Count = 50,
            TotalCount = 1000,
            Results = new System.Collections.Generic.List<PersonSearchResult>()
        };

        // Assert
        Assert.Equal(50, response.Limit);
        Assert.Equal(100, response.Offset);
        Assert.Equal(50, response.Count);
        Assert.Equal(1000, response.TotalCount);
        Assert.NotNull(response.Results);
    }

    #endregion
}
