# SixDegrees - Emby Plugin

An interactive force-directed graph visualization plugin for Emby that shows relationship connections between people (actors, directors, writers, producers, etc.) across all media in your library.

## Features

- **Person-to-Person Pathfinding**: Find the shortest connection path between any two people in your media library
- **Interactive Force-Directed Graph**: D3.js-powered visualization with drag, zoom, and pan capabilities
- **Configurable Depth**: Explore 1-6 degrees of separation from any person
- **All Media Types**: Includes movies, TV shows, and music albums
- **All Roles**: Counts all credited roles (actors, directors, writers, producers, composers, crew)
- **Smart Caching**: Pre-calculates relationship data for fast queries
- **Node Expansion**: Click on people or media to expand their connections

## Architecture

### Backend (C#)
- **SixDegreesPlugin**: Main plugin class
- **RelationshipGraphService**: Builds and maintains cached relationship graph
- **GraphCache**: In-memory graph structure for fast pathfinding
- **API Endpoints**: REST services for search and graph data

### Frontend (HTML/JavaScript)
- **D3.js**: Force-directed graph visualization
- **Search Interface**: Dual person search with autocomplete
- **Interactive Controls**: Depth slider, node expansion, filtering

## Visual Design

- **People**: Rendered as circles
- **Media**: Rendered as squares (movies, TV, albums)
- **Color Coding**: Media types distinguished by color
- **Images**: Profile photos displayed where available

## Installation

1. Build the project in Visual Studio
2. Copy the compiled DLL to your Emby Server plugins folder: `%AppData%\Emby-Server\programdata\plugins\`
3. Restart Emby Server
4. Navigate to the plugin page in the Emby web interface

## Development

### Prerequisites
- Visual Studio Code or Visual Studio 2019+
- .NET SDK 6.0+ (targets netstandard2.0)
- Emby Server Beta
- Node.js (for pre-commit hooks)

### Setup
1. Install dependencies:
```bash
npm install
```

This will automatically set up Git pre-commit hooks via Husky.

### Building
```bash
dotnet build
```

### Pre-Commit Hooks
This project uses Git pre-commit hooks to maintain code quality. The hooks run automatically on every commit and check:

1. **C# Formatting** - Validates code formatting with `dotnet format`
2. **C# Build** - Ensures the project compiles without errors
3. **JavaScript Linting** - Checks JavaScript code with ESLint
4. **Unit Tests** - Runs all tests to ensure they pass

If any check fails, the commit will be blocked. Fix the issues and try again.

**Bypassing hooks** (use sparingly):
```bash
git commit --no-verify -m "Your message"
```

**Manually run checks**:
```bash
# Format C# code
dotnet format 6degrees.sln

# Lint JavaScript
npm run lint

# Fix JavaScript issues automatically
npm run lint:fix
```

### Testing
Connect to your Emby Server and navigate to the "SixDegrees" page in the main menu.

## REST API Endpoints

The SixDegrees plugin exposes REST API endpoints following Emby's ServiceStack conventions. All endpoints support OpenAPI/Swagger documentation via the `[Route]`, `[Api]`, and `[ApiMember]` attributes.

### Base URL
All endpoints are accessible at: `http://your-emby-server:8096/SixDegrees/`

### Endpoints

#### 1. Get Statistics

Returns statistics about the relationship graph.

**Endpoint:** `GET /SixDegrees/Statistics`

**Response:**
```json
{
  "PeopleCount": 1000,
  "MediaCount": 500,
  "ConnectionCount": 5000,
  "LastBuildTime": "2026-01-21T12:00:00Z"
}
```

**Example:**
```bash
curl "http://localhost:8096/SixDegrees/Statistics"
```

---

#### 2. Search People

Search for people by name with pagination support.

**Endpoint:** `GET /SixDegrees/SearchPeople`

**Parameters:**

- `Query` (required): Search query string to match against person names
- `Limit` (optional): Maximum number of results to return (default: 20, max: 100)

**Response:**
```json
{
  "Query": "Tom",
  "Limit": 20,
  "Offset": 0,
  "Count": 2,
  "Results": [
    {
      "Id": "person-123",
      "Name": "Tom Hanks",
      "ImageUrl": "/emby/Items/person-123/Images/Primary",
      "ConnectionCount": 42
    },
    {
      "Id": "person-456",
      "Name": "Tom Cruise",
      "ImageUrl": "/emby/Items/person-456/Images/Primary",
      "ConnectionCount": 38
    }
  ]
}
```

**Example:**
```bash
curl "http://localhost:8096/SixDegrees/SearchPeople?Query=Tom&Limit=10"
```

---

#### 3. Get Graph Data

Get graph data (nodes and edges) centered on a person for D3.js visualization.

**Endpoint:** `GET /SixDegrees/Graph`

**Parameters:**

- `PersonId` (required): The ID of the person to center the graph on
- `Depth` (optional): Degrees of separation to include (1-6, default: 2)

**Response:**
```json
{
  "Success": true,
  "Message": null,
  "PersonId": "person-123",
  "Depth": 2,
  "Nodes": [
    {
      "Id": "person-123",
      "Name": "Tom Hanks",
      "Type": "person",
      "MediaType": null,
      "ImageUrl": "/emby/Items/person-123/Images/Primary",
      "Depth": 0
    },
    {
      "Id": "media-456",
      "Name": "Forrest Gump",
      "Type": "media",
      "MediaType": "Movie",
      "ImageUrl": "/emby/Items/media-456/Images/Primary",
      "Depth": 0
    }
  ],
  "Edges": [
    {
      "Source": "person-123",
      "Target": "media-456",
      "Role": "Actor"
    }
  ],
  "Truncated": false,
  "SearchTimeMs": 12.5,
  "NodesVisited": 10
}
```

**Example:**
```bash
curl "http://localhost:8096/SixDegrees/Graph?PersonId=person-123&Depth=3"
```

---

#### 4. Find Shortest Path

Find the shortest connection path between two people using BFS algorithm.

**Endpoint:** `GET /SixDegrees/ShortestPath`

**Parameters:**

- `FromPersonId` (required): The ID of the starting person
- `ToPersonId` (required): The ID of the target person

**Response:**
```json
{
  "Success": true,
  "Message": null,
  "Path": [
    {
      "Type": "person",
      "Id": "person-123",
      "Name": "Tom Hanks",
      "MediaType": null,
      "ImageUrl": "/emby/Items/person-123/Images/Primary",
      "Role": null
    },
    {
      "Type": "media",
      "Id": "media-456",
      "Name": "Forrest Gump",
      "MediaType": "Movie",
      "ImageUrl": "/emby/Items/media-456/Images/Primary",
      "Role": "Actor / Actor"
    },
    {
      "Type": "person",
      "Id": "person-789",
      "Name": "Gary Sinise",
      "MediaType": null,
      "ImageUrl": "/emby/Items/person-789/Images/Primary",
      "Role": null
    }
  ],
  "Degrees": 2,
  "SearchTimeMs": 5.0,
  "NodesVisited": 15
}
```

**Example:**
```bash
curl "http://localhost:8096/SixDegrees/ShortestPath?FromPersonId=person-123&ToPersonId=person-789"
```

---

#### 5. Get N-Degree Neighbors (Extended Graph)

Expands the graph from a person to N degrees of separation with configurable node limits.

**Endpoint:** `GET /SixDegrees/Neighbors`

**Parameters:**

- `PersonId` (required): The ID of the person to expand from
- `Degree` (optional): Degrees of separation to expand (1-6, default: 2)
- `MaxNodes` (optional): Maximum number of nodes to return (default: 500, max: 1000)

**Response:** Same structure as `/Graph` endpoint

**Example:**
```bash
curl "http://localhost:8096/SixDegrees/Neighbors?PersonId=person-123&Degree=3&MaxNodes=500"
```

---

#### 6. Get All People

Returns a paginated list of all people in the relationship graph.

**Endpoint:** `GET /SixDegrees/People`

**Parameters:**

- `Limit` (optional): Maximum number of results to return (default: 50, max: 200)
- `Offset` (optional): Number of results to skip for pagination (default: 0)

**Response:**
```json
{
  "Limit": 50,
  "Offset": 0,
  "Count": 50,
  "TotalCount": 1000,
  "Results": [
    {
      "Id": "person-123",
      "Name": "Tom Hanks",
      "ImageUrl": "/emby/Items/person-123/Images/Primary",
      "ConnectionCount": 42
    }
  ]
}
```

**Example:**
```bash
curl "http://localhost:8096/SixDegrees/People?Limit=100&Offset=200"
```

---

#### 7. Rebuild Cache

Triggers a rebuild of the relationship graph cache by scanning the entire media library.

**Endpoint:** `POST /SixDegrees/RebuildCache`

**Response:**
```json
{
  "Success": true,
  "Message": "Cache rebuilt successfully",
  "PeopleCount": 1000,
  "MediaCount": 500,
  "ConnectionCount": 5000,
  "BuildTimeMs": 1500.5
}
```

**Example:**
```bash
curl -X POST "http://localhost:8096/SixDegrees/RebuildCache"
```

---

### Error Handling

All endpoints return structured error responses with appropriate HTTP status codes:

**Validation Error:**
```json
{
  "Success": false,
  "Message": "PersonId is required"
}
```

**Not Found Error:**
```json
{
  "Success": false,
  "Message": "Person not found"
}
```

**Server Error:**
```json
{
  "Success": false,
  "Message": "Error during search: <error details>"
}
```

### OpenAPI/Swagger Support

The API endpoints are fully documented using ServiceStack attributes:

- `[Route]` - Defines the HTTP route and method
- `[Api]` - Provides summary and description for OpenAPI
- `[ApiMember]` - Documents request parameters with descriptions and constraints
- `IReturn<T>` - Specifies the response type for type-safe clients

These attributes enable automatic API documentation generation and client SDK generation through tools that support ServiceStack's metadata format.

### CORS Support

CORS is handled automatically by Emby Server's ServiceStack implementation. Cross-origin requests from the Emby web interface are supported by default.

## License

MIT

## Credits

Created with the Emby Beta SDK
