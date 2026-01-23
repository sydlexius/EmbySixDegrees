# Testing Guide

This document describes the unit testing framework and how to run tests for the SixDegrees plugin.

## Test Framework

The SixDegrees project uses the following testing frameworks and libraries:

- **xUnit** (v2.6.2) - Testing framework
- **Moq** (v4.20.70) - Mocking framework
- **FluentAssertions** (v6.12.0) - Assertion library for readable tests
- **coverlet.collector** (v6.0.0) - Code coverage collection

## Project Structure

```
SixDegrees.Tests/
├── ApiControllerTests.cs              # API request/response DTO tests
├── PathfindingServiceTests.cs         # BFS pathfinding algorithm tests
├── NeighborsServiceTests.cs           # N-degree neighbor expansion tests
├── RelationshipGraphTests.cs          # Graph data structure tests
├── Helpers/
│   └── GraphTestHelpers.cs            # Helper methods for creating test graphs
└── TestLogger.cs                      # No-op ILogger implementation
```

## Running Tests

### Run All Tests

```bash
dotnet test 6degrees.sln
```

### Run Tests with Coverage

```bash
dotnet test 6degrees.sln --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Generate Coverage Report

```bash
# Install reportgenerator (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/coveragereport" -reporttypes:"Html"

# Generate text summary
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/coveragereport" -reporttypes:"TextSummary"
```

## Current Test Coverage

As of the latest test run:

- **Overall Coverage**: 47.5% (532/1118 lines)
- **Branch Coverage**: 46.1% (107/232 branches)
- **Method Coverage**: 73.7% (143/194 methods)

### Coverage by Component

| Component | Coverage | Status |
|-----------|----------|--------|
| PathfindingService | 90.3% | ✅ Excellent |
| RelationshipGraph | 100% | ✅ Fully covered |
| API DTOs | 100% | ✅ Fully covered |
| Models | 100% | ✅ Fully covered |
| API Controller | 0% | ❌ Not tested (integration) |
| GraphService | 0% | ❌ Needs tests |
| Plugin | 0% | ❌ Integration only |

## Test Categories

### 1. Pathfinding Tests (PathfindingServiceTests.cs)

Tests for the BFS shortest path algorithm:

- **Direct Connections** (2 degrees)
- **Indirect Connections** (4+ degrees)
- **No Path Found** scenarios
- **Same Person** (0 degrees) validation
- **Error Handling** (missing persons, invalid input)
- **Performance Metrics** (search time, nodes visited)
- **Max Depth** enforcement
- **Optimal Path** selection (multiple paths)
- **Large Graph** performance

**Coverage**: 90.3% - Comprehensive coverage of core pathfinding logic

### 2. Neighbors Expansion Tests (NeighborsServiceTests.cs)

Tests for N-degree neighbor expansion:

- **Degree 1** (direct connections)
- **Degree 2+** (extended network)
- **Depth Tracking** for all nodes
- **Node Limit** enforcement (truncation)
- **Degree Clamping** (1-6 range)
- **Edge Validation** (role preservation)
- **Empty Graph** handling
- **Isolated Nodes** handling

**Coverage**: Part of PathfindingService (90.3%)

### 3. RelationshipGraph Tests (RelationshipGraphTests.cs)

Tests for the core graph data structure:

- **Adding Nodes** (people and media)
- **Creating Connections** (bidirectional relationships)
- **Querying Data** (GetPerson, GetMedia, searches)
- **Pagination** (limit and offset enforcement)
- **Input Validation** (null checks, argument validation)
- **Statistics** (counts, averages)
- **Thread Safety** (via locking)
- **Edge Cases** (duplicates, missing nodes, empty queries)

**Coverage**: 100% - Comprehensive coverage of all graph operations

### 4. API DTO Tests (ApiControllerTests.cs)

Tests for API request and response models:

- **Default Values** for all request types
- **Property Setters** for all DTOs
- **Response Structure** validation
- **Error Response** formatting

**Coverage**: 100% - All API models fully tested

## CI/CD Integration

Tests run automatically on every push and pull request via GitHub Actions.

### CI Workflow ([.github/workflows/ci.yml](.github/workflows/ci.yml))

```yaml
- name: Run tests
  run: dotnet test 6degrees.sln --configuration Debug --no-build --verbosity normal --collect:"XPlat Code Coverage"

- name: Upload coverage reports
  uses: codecov/codecov-action@v3
  if: success()
  with:
    files: '**/coverage.cobertura.xml'
    fail_ci_if_error: false
```

### Release Workflow ([.github/workflows/build.yml](.github/workflows/build.yml))

Tests also run on tagged releases to ensure quality before deployment.

## Writing New Tests

### Test Class Template

```csharp
using FluentAssertions;
using SixDegrees.Services;
using Xunit;

public class MyServiceTests
{
    private readonly TestLogger logger;
    private readonly MyService service;

    public MyServiceTests()
    {
        this.logger = new TestLogger();
        this.service = new MyService(this.logger);
    }

    [Fact]
    public void MyMethod_ValidInput_ReturnsExpected()
    {
        // Arrange
        var input = "test";

        // Act
        var result = this.service.MyMethod(input);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MyMethod_InvalidInput_ReturnsError(string input)
    {
        // Arrange & Act
        var result = this.service.MyMethod(input);

        // Assert
        result.Success.Should().BeFalse();
    }
}
```

### Using FluentAssertions

FluentAssertions provides more readable assertions:

```csharp
// Instead of:
Assert.Equal(expected, actual);

// Use:
actual.Should().Be(expected);

// Other examples:
result.Should().NotBeNull();
result.Success.Should().BeTrue();
result.Path.Should().HaveCount(3);
result.Path[0].Type.Should().Be("person");
result.Message.Should().Contain("error");
```

## Future Improvements

### High Priority

1. **RelationshipGraphService Tests**
   - Library scanning (Movies, TV, Music)
   - Cache save/load operations
   - Progress tracking
   - Error handling

2. **RelationshipGraph Tests**
   - AddPerson/AddMedia operations
   - Connection creation
   - Search functionality
   - Thread safety
   - Memory usage

### Medium Priority

3. **API Integration Tests**
   - Controller endpoint tests
   - Request validation
   - Error responses
   - Authentication (if added)

4. **Performance Tests**
   - Large graph performance (1000+ nodes)
   - Memory usage tracking
   - Concurrent access

### Low Priority

5. **Plugin Integration Tests**
   - Full lifecycle testing
   - Configuration changes
   - Emby server integration

## Test Data

### Test Helpers

The `GraphTestHelpers` class provides pre-built test graphs for various scenarios:

#### Standard Test Graph
A Hollywood-themed test graph with known connections:

- **Tom Hanks** → Forrest Gump → **Gary Sinise** (2 degrees)
- **Gary Sinise** → Apollo 13 → **Kevin Bacon** (4 degrees from Tom Hanks)
- **Kevin Bacon** → A Few Good Men → **Tom Cruise** (6 degrees from Tom Hanks)

This graph structure allows testing of:
- Direct connections (2 degrees)
- Multi-hop paths (4-6 degrees)
- Optimal path selection (when multiple paths exist)

#### Other Available Test Graphs

- **MinimalTestGraph**: Two people connected through one media item
- **LargeTestGraph**: Configurable large graph for performance testing
- **IsolatedNodesGraph**: Graph with both connected and isolated nodes
- **MultiplePathsGraph**: Diamond pattern with multiple paths between nodes

### Usage Example

```csharp
var graph = new RelationshipGraph(logger);
GraphTestHelpers.CreateStandardTestGraph(graph);
// Graph is now populated with test data
```

## Troubleshooting

### Tests Not Found

If `dotnet test` doesn't find tests:
```bash
dotnet clean
dotnet build
dotnet test
```

### Coverage Not Generated

Ensure coverlet.collector is installed:
```bash
dotnet add SixDegrees.Tests package coverlet.collector
```

### Test Failures After Code Changes

1. Review the failing test output
2. Check if API changes affected test expectations
3. Update test data if domain model changed
4. Verify mock setups are still valid

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
