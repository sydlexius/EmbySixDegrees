# Getting Started with Development

This guide will help you set up the development environment and start working on the Six Degrees plugin.

## Prerequisites

1. **Visual Studio 2019 or later** (or Visual Studio Code with C# extension)
2. **.NET Standard 2.0 SDK**
3. **Emby Server Beta** installed locally
4. **Emby Beta SDK** (referenced in project)

## Project Structure

```
6degrees/
├── SixDegrees.csproj          # Main project file
├── SixDegreesPlugin.cs        # Plugin entry point
├── Configuration/
│   └── PluginConfiguration.cs # Plugin settings
├── Models/
│   └── GraphNode.cs           # Data models
├── Services/
│   └── RelationshipGraphService.cs  # Graph building (TODO)
├── Api/
│   └── SixDegreesService.cs   # REST endpoints (TODO)
├── wwwroot/                   # Frontend files (TODO)
│   ├── index.html
│   ├── css/
│   └── js/
├── README.md                  # Project overview
├── ARCHITECTURE.md            # Technical architecture
├── MILESTONES.md              # Development milestones
├── ISSUES.md                  # Issue tracking
└── GETTING_STARTED.md         # This file
```

## Building the Project

### Step 1: Restore NuGet Packages

```bash
cd D:\Dev\Repos\Emby\6degrees
dotnet restore
```

### Step 2: Build

```bash
dotnet build
```

The post-build event will automatically copy the DLL to:
```
%AppData%\Emby-Server\programdata\plugins\
```

### Step 3: Restart Emby Server

After building, restart your local Emby Server to load the plugin.

### Step 4: Verify Plugin Loaded

1. Open Emby web interface
2. Navigate to **Settings** → **Plugins**
3. Look for "Six Degrees of Separation" in the plugin list

## Development Workflow

### Working on a Milestone

1. Review the milestone tasks in [MILESTONES.md](MILESTONES.md)
2. Check related issues in [ISSUES.md](ISSUES.md)
3. Create feature branch (optional): `git checkout -b feature/milestone-X`
4. Implement the tasks
5. Build and test with local Emby Server
6. Commit changes with descriptive messages

### Testing Changes

After each build:
1. DLL is auto-copied to Emby plugins folder
2. Restart Emby Server
3. Check Emby logs for any errors:
   - Location: `%AppData%\Emby-Server\logs\`
   - Look for "Six Degrees" messages

### Debugging

#### Visual Studio
1. Build project in Debug configuration
2. Attach debugger to `EmbyServer.exe` process
3. Set breakpoints in your code
4. Trigger plugin functionality in Emby

#### Logging
Use the injected logger to add diagnostic messages:

```csharp
this.logger.Info("Message");
this.logger.Debug("Detailed info");
this.logger.Warn("Warning message");
this.logger.Error("Error occurred");
```

## Current Development Status

### ✅ Completed
- Project structure created
- Basic plugin class implemented
- Configuration model defined
- Graph data models created
- Documentation written

### 🚧 In Progress
- None currently

### ⏳ TODO (Next Steps)
1. **Implement RelationshipGraphService**
   - Create `Services/RelationshipGraphService.cs`
   - Implement library scanning
   - Build bidirectional graph
   - Add logging

2. **Create API Endpoints**
   - Create `Api/SixDegreesService.cs`
   - Implement SearchPeople endpoint
   - Implement GetGraph endpoint
   - Implement ShortestPath endpoint

3. **Build Frontend**
   - Create HTML page structure
   - Add D3.js visualization
   - Implement search interface
   - Add interactive controls

See [MILESTONES.md](MILESTONES.md) for detailed task breakdown.

## Helpful Resources

### Emby SDK Documentation
- Main docs: Check `D:\Dev\Repos\Emby.SDK\Documentation\`
- Plugin UI: `doc/plugins/ui/index.html`
- API Endpoints: `doc/plugins/dev/Creating-Api-Endpoints.html`
- Sample Code: `SampleCode/Examples/`

### External Resources
- [D3.js Documentation](https://d3js.org/)
- [D3 Force Layout](https://github.com/d3/d3-force)
- [Emby API Documentation](https://github.com/MediaBrowser/Emby)

## Common Issues

### Issue: Plugin doesn't appear in Emby
**Solution**:
- Check that DLL was copied to plugins folder
- Restart Emby Server
- Check Emby logs for loading errors

### Issue: Build fails with "MediaBrowser not found"
**Solution**:
- Run `dotnet restore`
- Check NuGet package source includes Emby packages

### Issue: Changes not reflected after rebuild
**Solution**:
- Ensure Emby Server is stopped before copying new DLL
- Delete old DLL from plugins folder
- Build and copy fresh DLL
- Restart Emby

## Next Steps

To start development, choose a milestone from [MILESTONES.md](MILESTONES.md) and begin implementing the tasks. The recommended order is:

1. **Milestone 1**: Complete project setup (verify plugin loads)
2. **Milestone 2**: Implement RelationshipGraphService
3. **Milestone 3**: Implement pathfinding algorithms
4. **Milestone 4**: Create REST API endpoints
5. Continue through remaining milestones...

## Questions?

- Review the [ARCHITECTURE.md](ARCHITECTURE.md) for technical details
- Check [ISSUES.md](ISSUES.md) for specific implementation tasks
- Look at Emby SDK sample code for examples

Happy coding!
