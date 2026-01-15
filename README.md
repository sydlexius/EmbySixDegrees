# Six Degrees of Separation - Emby Plugin

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
- Visual Studio 2019 or later
- .NET Standard 2.0
- Emby Server Beta SDK

### Building
```bash
dotnet build
```

### Testing
Connect to your Emby Server and navigate to the "Six Degrees" page in the main menu.

## API Endpoints

- `GET /SixDegrees/SearchPeople?query={name}` - Search for people by name
- `GET /SixDegrees/Graph?personId={id}&depth={1-6}` - Get graph data for visualization
- `GET /SixDegrees/ShortestPath?from={id}&to={id}` - Find shortest path between two people
- `POST /SixDegrees/RebuildCache` - Trigger cache rebuild

## License

MIT

## Credits

Created with the Emby Beta SDK
