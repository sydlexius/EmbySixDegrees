# Development Issues & Tracking

## Active Issues

### Issue #1: Project Setup
**Priority**: High
**Milestone**: 1 - Project Setup & Foundation
**Status**: In Progress

**Description**: Create the basic C# project structure with Emby SDK dependencies.

**Tasks**:
- [ ] Create SixDegrees.csproj file
- [ ] Add MediaBrowser.Server.Core NuGet package reference
- [ ] Create basic plugin class implementing BasePlugin
- [ ] Configure post-build event to copy DLL to Emby plugins folder
- [ ] Test that plugin loads in Emby Server

**Acceptance Criteria**:
- Project builds without errors
- Plugin appears in Emby Server plugin list
- Plugin can be enabled/disabled without crashing

---

### Issue #2: Graph Data Structures
**Priority**: High
**Milestone**: 2 - Relationship Graph Service
**Status**: Not Started

**Description**: Design and implement the core graph data structures for storing person-media relationships.

**Tasks**:
- [ ] Create PersonNode class
- [ ] Create MediaNode class
- [ ] Create Connection/Edge classes
- [ ] Implement bidirectional adjacency list
- [ ] Add memory-efficient storage for large graphs
- [ ] Write unit tests for data structures

**Technical Notes**:
```csharp
// Proposed structure
public class PersonNode {
    public string Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public Dictionary<string, MediaConnection> MediaConnections { get; set; }
}

public class MediaConnection {
    public string MediaId { get; set; }
    public string MediaName { get; set; }
    public string MediaType { get; set; } // Movie, Series, Album
    public string Role { get; set; }       // Actor, Director, etc.
}
```

---

### Issue #3: Library Scanning
**Priority**: High
**Milestone**: 2 - Relationship Graph Service
**Status**: Not Started

**Description**: Implement the library scanning logic to extract people from all media items.

**Tasks**:
- [ ] Inject ILibraryManager dependency
- [ ] Query all Movie items from library
- [ ] Query all Series items from library
- [ ] Query all MusicAlbum items from library
- [ ] Extract People metadata from each item
- [ ] Handle missing metadata gracefully
- [ ] Add progress logging

**Technical Considerations**:
- Use async/await for library queries
- Process in batches to avoid memory spikes
- Handle null/missing People data
- Consider incremental scanning for large libraries

**Emby API Reference**:
```csharp
var query = new InternalItemsQuery {
    IncludeItemTypes = new[] { "Movie", "Series", "MusicAlbum" },
    Recursive = true
};
var items = libraryManager.GetItemList(query);
```

---

### Issue #4: BFS Shortest Path Algorithm
**Priority**: High
**Milestone**: 3 - Pathfinding Algorithms
**Status**: Not Started

**Description**: Implement breadth-first search to find shortest connection path between two people.

**Tasks**:
- [ ] Implement BFS traversal
- [ ] Track parent nodes for path reconstruction
- [ ] Handle no-path-found cases
- [ ] Add maximum search depth limit
- [ ] Optimize for performance
- [ ] Write unit tests with known paths

**Algorithm Pseudocode**:
```
BFS(startPerson, targetPerson):
  queue = [startPerson]
  visited = {startPerson}
  parent = {}

  while queue not empty:
    current = queue.dequeue()

    if current == targetPerson:
      return reconstructPath(parent, startPerson, targetPerson)

    for each media in current.mediaConnections:
      for each person in media.people:
        if person not in visited:
          visited.add(person)
          parent[person] = {via: media, from: current}
          queue.enqueue(person)

  return null
```

---

### Issue #5: REST API Design
**Priority**: Medium
**Milestone**: 4 - REST API Endpoints
**Status**: Not Started

**Description**: Design and implement REST API endpoints following Emby conventions.

**Tasks**:
- [ ] Define request/response DTOs
- [ ] Implement IService interface
- [ ] Add Route attributes
- [ ] Add API documentation attributes
- [ ] Implement error responses
- [ ] Add request validation

**API Spec**:
```csharp
[Route("/SixDegrees/SearchPeople", "GET")]
[Api(Description = "Search for people by name")]
public class SearchPeople : IReturn<PersonSearchResult[]> {
    public string Query { get; set; }
    public int Limit { get; set; } = 20;
}

[Route("/SixDegrees/ShortestPath", "GET")]
[Api(Description = "Find shortest path between two people")]
public class GetShortestPath : IReturn<PathResult> {
    public string FromPersonId { get; set; }
    public string ToPersonId { get; set; }
}
```

---

### Issue #6: Frontend HTML Structure
**Priority**: Medium
**Milestone**: 5 - Frontend Foundation
**Status**: Not Started

**Description**: Create the HTML page structure for the plugin UI.

**Tasks**:
- [ ] Create index.html file
- [ ] Add D3.js CDN reference (v7)
- [ ] Create search panel HTML
- [ ] Create graph container SVG
- [ ] Create control panel HTML
- [ ] Add CSS file for styling
- [ ] Create main.js for initialization

**File Structure**:
```
wwwroot/
├── index.html
├── css/
│   └── sixdegrees.css
└── js/
    ├── main.js
    ├── api-client.js
    ├── search.js
    └── graph.js
```

---

### Issue #7: D3.js Force Simulation
**Priority**: Medium
**Milestone**: 7 - Force-Directed Graph Visualization
**Status**: Not Started

**Description**: Implement the core D3.js force-directed graph visualization.

**Tasks**:
- [ ] Set up force simulation with appropriate forces
- [ ] Render person nodes as circles
- [ ] Render media nodes as squares
- [ ] Render edges/links
- [ ] Add drag behavior
- [ ] Add zoom/pan behavior
- [ ] Style nodes and edges

**D3 Configuration**:
```javascript
const simulation = d3.forceSimulation(nodes)
  .force("link", d3.forceLink(links).distance(100))
  .force("charge", d3.forceManyBody().strength(-500))
  .force("center", d3.forceCenter(width/2, height/2))
  .force("collide", d3.forceCollide().radius(30));
```

---

### Issue #8: Cache Refresh Strategy
**Priority**: Low
**Milestone**: 2 - Relationship Graph Service
**Status**: Not Started

**Description**: Implement background cache refresh to keep graph data up to date.

**Tasks**:
- [ ] Add IScheduledTask implementation
- [ ] Configure refresh interval
- [ ] Implement incremental updates
- [ ] Add cache invalidation on library changes
- [ ] Test refresh doesn't impact performance

**Options to Consider**:
1. Periodic full rebuild (simple, but slow)
2. Incremental updates on library changes (complex, but efficient)
3. Hybrid: incremental + periodic validation

---

### Issue #9: Large Library Performance
**Priority**: Medium
**Milestone**: 11 - Testing & Optimization
**Status**: Not Started

**Description**: Ensure plugin performs well with large libraries (10,000+ items).

**Tasks**:
- [ ] Profile memory usage with large library
- [ ] Optimize graph building performance
- [ ] Implement result pagination
- [ ] Add node/edge limits for visualization
- [ ] Test with user's actual library
- [ ] Document performance characteristics

**Performance Targets**:
- Graph build: < 30 seconds for 10,000 items
- Memory usage: < 200MB for 10,000 items
- Shortest path query: < 100ms
- Graph render: < 2 seconds for 500 nodes

---

### Issue #10: Error Handling
**Priority**: Medium
**Milestone**: 11 - Testing & Optimization
**Status**: Not Started

**Description**: Add comprehensive error handling throughout the plugin.

**Tasks**:
- [ ] Handle missing library data gracefully
- [ ] Add try-catch blocks in API endpoints
- [ ] Display user-friendly error messages in UI
- [ ] Log errors appropriately
- [ ] Handle network failures
- [ ] Add fallbacks for missing images

**Error Scenarios to Handle**:
- Person not found
- No path exists between two people
- Library scan fails
- API request fails
- Graph data too large to visualize
- Browser doesn't support required features

---

## Future Enhancements (Backlog)

### Enhancement #1: Disk-Based Caching
**Priority**: Low
**Description**: Serialize graph to disk to speed up server restarts.

### Enhancement #2: Graph Statistics
**Priority**: Low
**Description**: Show interesting statistics like most connected person, average degrees, etc.

### Enhancement #3: Export Functionality
**Priority**: Low
**Description**: Export graph as PNG image or JSON data file.

### Enhancement #4: Advanced Filters
**Priority**: Low
**Description**: Filter by year range, specific roles, media types, etc.

### Enhancement #5: Bacon Number Display
**Priority**: Low
**Description**: Show a specific person's "Bacon Number" or connection degree.

---

## Bug Reports

### Bug #1: [Placeholder]
**Priority**: TBD
**Status**: None reported yet
**Description**: Bugs will be tracked here as they are discovered during development and testing.

---

## Notes

- Issues are organized by milestone and priority
- Each issue should be atomic and testable
- Issues can be converted to GitHub Issues once repository is created
- Priority levels: High (blocking), Medium (important), Low (nice-to-have)
