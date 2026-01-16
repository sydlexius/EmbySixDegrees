# Architecture Document

## System Overview

The SixDegrees plugin consists of three main layers:

1. **Data Collection Layer**: Scans the Emby library to build a relationship graph
2. **API Layer**: Exposes REST endpoints for querying relationship data
3. **Visualization Layer**: Renders interactive force-directed graphs using D3.js

## Data Model

### Graph Structure

```
Person Node {
    Id: string (Emby Person ID)
    Name: string
    Type: "Person"
    ImageUrl: string (optional)
    Connections: List<MediaConnection>
}

Media Node {
    Id: string (Emby Item ID)
    Name: string
    Type: "Movie" | "Series" | "Album"
    Year: int
    ImageUrl: string (optional)
    People: List<PersonConnection>
}

Connection {
    TargetId: string
    Role: string (Actor, Director, Writer, Producer, etc.)
}
```

### In-Memory Cache

The `RelationshipGraphService` maintains two primary data structures:

1. **Person-to-Media Adjacency List**: `Dictionary<PersonId, List<MediaConnection>>`
2. **Media-to-Person Adjacency List**: `Dictionary<MediaId, List<PersonConnection>>`

This bidirectional graph allows for efficient traversal in both directions.

## Components

### 1. RelationshipGraphService

**Responsibilities:**
- Build relationship graph on startup
- Periodic cache refresh (configurable interval)
- Provide graph traversal methods
- Implement breadth-first search for shortest paths

**Key Methods:**
- `BuildGraphAsync()`: Scans library and builds cache
- `FindShortestPath(fromPersonId, toPersonId)`: BFS pathfinding
- `GetNeighbors(personId, depth)`: Get N-degree connections
- `SearchPeople(query)`: Search for people by name

### 2. API Endpoints

#### SearchPeople
```
GET /SixDegrees/SearchPeople?query={name}&limit={20}
Response: PersonSearchResult[]
```

#### GetGraph
```
GET /SixDegrees/Graph?personId={id}&depth={1-6}
Response: GraphData { nodes: [], edges: [] }
```

#### ShortestPath
```
GET /SixDegrees/ShortestPath?from={id}&to={id}
Response: PathResult { path: [], degrees: int }
```

#### RebuildCache
```
POST /SixDegrees/RebuildCache
Response: StatusResult
```

### 3. Frontend Visualization

**Technologies:**
- D3.js v7 for force simulation
- Vanilla JavaScript (ES6+)
- CSS3 for styling

**Components:**
- `SearchPanel`: Dual search interface for selecting two people
- `GraphVisualization`: D3.js force-directed graph
- `ControlPanel`: Depth slider, filters, options
- `InfoPanel`: Display node details on hover/click

**Force Simulation Parameters:**
```javascript
{
  charge: -500,           // Node repulsion
  linkDistance: 100,      // Edge length
  collideRadius: 30,      // Collision detection
  centerStrength: 0.1     // Center gravity
}
```

## Algorithms

### Shortest Path (BFS)

```
function FindShortestPath(fromPersonId, toPersonId):
    queue = new Queue()
    visited = new Set()
    parent = new Map()

    queue.enqueue(fromPersonId)
    visited.add(fromPersonId)

    while queue is not empty:
        current = queue.dequeue()

        if current == toPersonId:
            return reconstructPath(parent, fromPersonId, toPersonId)

        for each media in person[current].media:
            for each person in media.people:
                if person not in visited:
                    visited.add(person)
                    parent[person] = {media, current}
                    queue.enqueue(person)

    return null // No path found
```

### N-Degree Expansion

```
function GetNeighbors(personId, maxDepth):
    nodes = new Set()
    edges = new Set()
    queue = new Queue()

    queue.enqueue({id: personId, depth: 0})

    while queue is not empty:
        {id, depth} = queue.dequeue()

        if depth >= maxDepth:
            continue

        for each media in person[id].media:
            nodes.add(media)
            edges.add({from: id, to: media.id, role: role})

            for each connectedPerson in media.people:
                if connectedPerson not in nodes:
                    nodes.add(connectedPerson)
                    edges.add({from: media.id, to: connectedPerson.id, role: role})
                    queue.enqueue({id: connectedPerson.id, depth: depth + 1})

    return {nodes, edges}
```

## Performance Considerations

### Cache Building
- **Estimated Time**: 10-30 seconds for 10,000 media items
- **Memory Usage**: ~50-100MB for 10,000 items with 50,000 people
- **Strategy**: Build on startup, refresh periodically (default: 1 hour)

### Query Performance
- **Shortest Path**: O(V + E) where V = people, E = connections
- **N-Degree Expansion**: O(B^D) where B = branching factor, D = depth
- **Mitigation**: Limit max depth to 6, implement result limits

### Scalability
- Large libraries (>50,000 items) may require disk-based caching
- Consider implementing LRU cache for frequently requested paths
- Add configurable result limits to prevent UI overload

## Configuration Options

```csharp
public class SixDegreesConfiguration
{
    public int CacheRefreshIntervalMinutes { get; set; } = 60;
    public int MaxSearchResults { get; set; } = 50;
    public int MaxGraphNodes { get; set; } = 500;
    public bool IncludeMusic { get; set; } = true;
    public bool IncludeTV { get; set; } = true;
    public bool IncludeMovies { get; set; } = true;
}
```

## Future Enhancements

1. **Disk-based Caching**: Serialize graph to disk for faster startup
2. **Incremental Updates**: Update cache as new media is added
3. **Statistics**: Show interesting facts (most connected person, etc.)
4. **Export**: Save graphs as images or data files
5. **Filters**: Filter by media type, year range, role type
6. **Dark Mode**: Theme support for visualization
