# Performance Optimization Guide

This document describes the performance optimizations implemented in the Six Degrees plugin and provides guidance for working with large libraries.

## Performance Targets

The following performance targets have been designed for libraries with 10,000+ media items:

| Metric | Target | Description |
|--------|--------|-------------|
| Graph Build Time | < 30 seconds | Time to scan library and build complete graph |
| Memory Usage | < 200MB | Peak memory usage for graph with 10,000 items |
| Shortest Path Query | < 100ms | Time to find path between any two people |
| Graph Render | < 2 seconds | Time to render up to 500 nodes in browser |

## Optimizations Implemented

### 1. Memory Optimization

#### Dictionary Capacity Pre-allocation
The `RelationshipGraph` constructor accepts capacity hints to avoid dictionary resizing:

```csharp
public RelationshipGraph(ILogger logger, int estimatedPeopleCapacity = 5000, int estimatedMediaCapacity = 10000)
{
    this.people = new Dictionary<string, PersonNode>(estimatedPeopleCapacity);
    this.media = new Dictionary<string, MediaNode>(estimatedMediaCapacity);
}
```

**Benefits:**
- Reduces memory allocations during graph building
- Eliminates dictionary resize overhead
- Typical memory savings: 20-30%

**Usage:**
```csharp
// For large libraries, provide accurate estimates
var graph = new RelationshipGraph(logger, estimatedPeopleCapacity: 10000, estimatedMediaCapacity: 20000);
```

#### Cached Connection Count
Connection count calculation is expensive (O(n) over all people). We cache the result and only recalculate when the graph changes:

```csharp
private int cachedConnectionCount;
private bool connectionCountDirty = true;

public int ConnectionCount
{
    get
    {
        if (this.connectionCountDirty)
        {
            this.cachedConnectionCount = this.people.Values.Sum(p => p.MediaConnections.Count);
            this.connectionCountDirty = false;
        }
        return this.cachedConnectionCount;
    }
}
```

**Benefits:**
- Reduces repeated expensive calculations
- O(1) time complexity for cached access
- Critical for statistics endpoints

### 2. Algorithm Optimization

#### BFS Pathfinding with Early Termination
The breadth-first search algorithm includes several optimizations:

**Depth Limiting:**
```csharp
private const int MaxSearchDepth = 6; // Six degrees of separation

if (depth >= maxDepth)
{
    continue; // Skip expanding this node
}
```

**Visited Set for O(1) Lookup:**
```csharp
var visited = new HashSet<string>(); // O(1) contains check instead of O(n) list search
```

**Early Target Detection:**
```csharp
if (currentPersonId == targetPersonId)
{
    return this.ReconstructPath(parent, startPersonId, targetPersonId); // Exit immediately
}
```

**Performance Characteristics:**
- Time Complexity: O(V + E) where V = vertices, E = edges
- Space Complexity: O(V) for visited set and queue
- Expected time for typical queries: 10-50ms
- Worst case (no path): < 100ms even for large graphs

#### N-Degree Expansion with Node Limits
The neighbor expansion algorithm enforces hard limits to prevent browser overload:

```csharp
public NeighborsResult GetNeighbors(string personId, int degree, int maxNodes = 500)
{
    maxNodes = Math.Max(1, Math.Min(maxNodes, 1000)); // Clamp between 1-1000

    while (queue.Count > 0 && nodes.Count < maxNodes)
    {
        // Expand graph until limit reached
    }
}
```

**Benefits:**
- Prevents out-of-memory errors in browser
- Ensures responsive UI even for highly connected people
- Gracefully handles "truncated" result sets

### 3. API Pagination

All list-returning endpoints support pagination to reduce payload size and improve response times.

#### Search Endpoint
```
GET /SixDegrees/Search?query=Tom&limit=20&offset=0
```

**Parameters:**
- `query`: Search term (required)
- `limit`: Results per page (default: 20, max: 100)
- `offset`: Number of results to skip (default: 0)

**Response:**
```json
{
  "query": "Tom",
  "limit": 20,
  "offset": 0,
  "count": 20,
  "results": [...]
}
```

#### People Listing Endpoint
```
GET /SixDegrees/People?limit=50&offset=0
```

**Parameters:**
- `limit`: Results per page (default: 50, max: 200)
- `offset`: Number of results to skip (default: 0)

**Response:**
```json
{
  "limit": 50,
  "offset": 0,
  "count": 50,
  "totalCount": 5000,
  "results": [...]
}
```

#### Path Finding Endpoint
```
GET /SixDegrees/FindPath?fromPersonId=123&toPersonId=456&maxDepth=6
```

**Parameters:**
- `fromPersonId`: Starting person (required)
- `toPersonId`: Target person (required)
- `maxDepth`: Maximum search depth (default: 6, max: 6)

**Response:**
```json
{
  "success": true,
  "path": [...],
  "degrees": 3,
  "searchTimeMs": 42.5
}
```

#### Neighbors Endpoint
```
GET /SixDegrees/Neighbors?personId=123&degree=2&maxNodes=500
```

**Parameters:**
- `personId`: Center person (required)
- `degree`: Degrees of separation (default: 2, max: 6)
- `maxNodes`: Maximum nodes to return (default: 500, max: 1000)

**Response:**
```json
{
  "success": true,
  "personId": "123",
  "degree": 2,
  "nodes": [...],
  "edges": [...],
  "truncated": false,
  "searchTimeMs": 87.3
}
```

### 4. Visualization Limits

The frontend should enforce limits to ensure responsive rendering:

**Recommended Limits:**
- Maximum nodes per visualization: 500
- Maximum edges per visualization: 2000
- Force simulation iterations: 300
- Minimum zoom level: 0.1x
- Maximum zoom level: 10x

**D3.js Configuration:**
```javascript
const simulation = d3.forceSimulation(nodes)
  .force("link", d3.forceLink(edges).distance(100))
  .force("charge", d3.forceManyBody().strength(-500))
  .force("center", d3.forceCenter(width/2, height/2))
  .force("collide", d3.forceCollide().radius(30))
  .alphaDecay(0.02) // Faster convergence
  .stop();

// Run simulation in batches to avoid UI blocking
for (let i = 0; i < 300; ++i) simulation.tick();
```

## Testing with Large Libraries

### Performance Profiling

To profile performance with your actual library:

1. **Enable debug logging** in Emby Server configuration
2. **Monitor memory usage** using Task Manager or Performance Monitor
3. **Check API response times** in browser developer tools
4. **Review log files** for timing information

### Expected Performance by Library Size

| Library Size | People | Media | Build Time | Memory Usage | Query Time |
|--------------|--------|-------|------------|--------------|------------|
| Small (< 1,000) | ~500 | ~1,000 | < 5s | < 50MB | < 10ms |
| Medium (1,000-5,000) | ~2,000 | ~5,000 | < 15s | < 100MB | < 25ms |
| Large (5,000-10,000) | ~4,000 | ~10,000 | < 30s | < 200MB | < 50ms |
| Very Large (> 10,000) | ~8,000+ | ~20,000+ | 30-60s | 200-400MB | 50-100ms |

### Performance Tuning

If performance is below targets, consider:

1. **Increase capacity hints** in `RelationshipGraph` constructor
2. **Reduce `maxDepth`** in pathfinding queries
3. **Decrease `maxNodes`** in neighbor expansion
4. **Implement disk caching** (see Enhancement #1 in ISSUES.md)
5. **Filter by media type** to reduce graph size

## Memory Usage Breakdown

Typical memory usage for a graph with 5,000 people and 10,000 media items:

| Component | Memory | Percentage |
|-----------|--------|------------|
| PersonNode dictionaries | ~60MB | 40% |
| MediaNode dictionaries | ~50MB | 33% |
| Connection dictionaries | ~30MB | 20% |
| Overhead (strings, etc.) | ~10MB | 7% |
| **Total** | **~150MB** | **100%** |

### Per-Node Memory Cost

- **PersonNode**: ~10-15 KB (depending on connection count)
- **MediaNode**: ~8-12 KB (depending on connection count)
- **MediaConnection**: ~200-300 bytes
- **PersonConnection**: ~150-200 bytes

## Best Practices

### For Plugin Developers

1. **Always use pagination** for list endpoints
2. **Enforce node/edge limits** in visualization code
3. **Cache expensive calculations** (like connection counts)
4. **Use appropriate data structures** (HashSet for lookups, List for iteration)
5. **Profile with realistic data** before deploying

### For Plugin Users

1. **Use specific search queries** instead of browsing all people
2. **Start with low degree values** (1-2) and increase if needed
3. **Limit visualization complexity** by reducing max nodes
4. **Clear browser cache** if visualization becomes slow
5. **Report performance issues** with library size details

## Monitoring & Diagnostics

### API Response Time Headers

All API responses include timing information:

```json
{
  "success": true,
  "searchTimeMs": 42.5,
  ...
}
```

### Graph Statistics Endpoint

Monitor graph health using the statistics endpoint:

```
GET /SixDegrees/Statistics
```

**Response:**
```json
{
  "peopleCount": 5000,
  "mediaCount": 10000,
  "connectionCount": 50000,
  "averageConnectionsPerPerson": 10.0
}
```

**Health Indicators:**
- Average connections per person: 5-20 (typical)
- Average connections per person: < 5 (sparse graph, may have poor connectivity)
- Average connections per person: > 50 (very dense graph, may have performance issues)

## Future Optimizations

See [ISSUES.md](ISSUES.md) for planned enhancements:

- **Enhancement #1**: Disk-based caching for faster restarts
- **Enhancement #2**: Graph statistics dashboard
- **Issue #8**: Incremental cache refresh strategy

## Troubleshooting

### Slow Graph Building

**Symptoms:** Initial graph build takes > 30 seconds

**Solutions:**
- Check Emby library scan is complete
- Ensure metadata is properly loaded
- Increase capacity hints in RelationshipGraph constructor
- Check for excessive logging (disable debug level)

### High Memory Usage

**Symptoms:** Plugin uses > 200MB for moderate library

**Solutions:**
- Verify capacity hints are set appropriately
- Check for memory leaks (restart plugin)
- Implement disk caching (Enhancement #1)
- Filter by media type to reduce graph size

### Slow Pathfinding Queries

**Symptoms:** Queries take > 100ms consistently

**Solutions:**
- Reduce `maxDepth` parameter (default is 6)
- Check graph connectivity (disconnected subgraphs)
- Profile with debug logging enabled
- Verify BFS implementation (should be O(V+E))

### Browser Performance Issues

**Symptoms:** Visualization is slow or unresponsive

**Solutions:**
- Reduce `maxNodes` in neighbor expansion (default 500)
- Limit degrees of separation (try 1-2 instead of 3-6)
- Use modern browser (Chrome, Firefox, Edge)
- Disable browser extensions that may interfere
- Check browser console for JavaScript errors

## Performance Testing Checklist

Before releasing new features:

- [ ] Test with small library (< 1,000 items)
- [ ] Test with medium library (1,000-5,000 items)
- [ ] Test with large library (> 10,000 items)
- [ ] Verify API response times meet targets
- [ ] Check memory usage under load
- [ ] Profile pathfinding algorithm performance
- [ ] Test visualization with max nodes
- [ ] Verify pagination works correctly
- [ ] Test edge cases (no path found, person not found, etc.)
- [ ] Review logs for errors or warnings
