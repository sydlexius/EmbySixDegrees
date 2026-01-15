# Caching Strategy & Performance

## Problem Statement

The relationship graph needs to be:
1. **Fast to query** - Pathfinding should return results in < 100ms
2. **Memory efficient** - Handle libraries with 10,000+ media items
3. **Persistent** - Survive server restarts without full rebuild
4. **Up-to-date** - Reflect new media additions

## Storage Options Analysis

### Option 1: In-Memory Only (Current Design)
**Pros:**
- Fastest query performance (all data in RAM)
- Simple implementation
- No disk I/O overhead

**Cons:**
- Lost on server restart (requires full rebuild)
- Memory intensive for large libraries
- Rebuild time: 10-30 seconds for 10,000 items

**Best for:** Small-medium libraries (< 5,000 items)

### Option 2: Disk-Based with In-Memory Cache
**Pros:**
- Survives server restarts
- Fast startup (load from disk)
- Can handle larger libraries

**Cons:**
- More complex implementation
- Serialization/deserialization overhead
- Need cache invalidation strategy

**Best for:** Medium-large libraries (5,000-50,000 items)

### Option 3: Hybrid Approach (RECOMMENDED)
**Implementation:**
1. Build graph in memory on first run
2. Serialize to disk on completion
3. On startup: load from disk if exists and recent
4. Scheduled task: rebuild periodically
5. Incremental updates on library changes (future enhancement)

**Structure:**
```
%AppData%\Emby-Server\programdata\plugins\SixDegrees\
├── graph-cache.json          # Serialized graph data
├── metadata.json             # Cache version, build time, stats
└── logs\                     # Plugin-specific logs
```

## Recommended Architecture

### Data Storage Format

**File: graph-cache.json**
```json
{
  "version": "1.0",
  "buildTimestamp": "2026-01-14T10:30:00Z",
  "statistics": {
    "totalPeople": 5420,
    "totalMedia": 2103,
    "totalConnections": 15678,
    "buildDurationMs": 12456
  },
  "people": {
    "person-id-1": {
      "id": "person-id-1",
      "name": "Tom Hanks",
      "imageUrl": "/emby/Items/person-id-1/Images/Primary",
      "media": {
        "movie-id-1": {
          "mediaId": "movie-id-1",
          "mediaName": "Forrest Gump",
          "mediaType": "Movie",
          "role": "Actor",
          "year": 1994
        }
      }
    }
  }
}
```

### Performance Optimization

#### 1. Lazy Loading
- Load full graph on first query only
- Keep metadata in memory always
- Deserialize on-demand if not in memory

#### 2. Compression
- Use GZip compression for disk storage
- Reduces file size by ~70%
- Trade CPU time for disk I/O

#### 3. Indexing
- Maintain separate index files for fast lookups:
  - `person-index.json` - Person ID → Name mapping
  - `media-index.json` - Media ID → Name mapping

### Scheduled Task Implementation

```csharp
public class RebuildGraphTask : IScheduledTask
{
    public string Name => "Rebuild Six Degrees Graph";
    public string Description => "Rebuilds the relationship graph cache";
    public string Category => "Six Degrees";
    public string Key => "SixDegreesRebuildGraph";

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[] {
            // Daily at 3 AM
            new TaskTriggerInfo {
                Type = TaskTriggerInfo.TriggerDaily,
                TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
            }
        };
    }

    public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
    {
        var service = SixDegreesPlugin.Instance.GetGraphService();
        await service.RebuildGraphAsync(progress, cancellationToken);
    }
}
```

### Cache Invalidation Strategy

**Triggers for Rebuild:**
1. Manual trigger (API endpoint or scheduled task)
2. Daily automatic rebuild (scheduled task)
3. After X new items added (future: incremental update)

**Validation:**
```csharp
public bool IsCacheValid()
{
    if (!File.Exists(cacheFilePath))
        return false;

    var metadata = LoadMetadata();
    var age = DateTime.UtcNow - metadata.BuildTimestamp;

    // Invalid if older than configured interval
    if (age.TotalMinutes > Configuration.CacheRefreshIntervalMinutes)
        return false;

    // Invalid if library has changed significantly
    var currentItemCount = libraryManager.GetItemList(query).Count;
    if (Math.Abs(currentItemCount - metadata.TotalMedia) > 100)
        return false;

    return true;
}
```

## Memory Estimates

### Small Library (1,000 movies, 2,000 people)
- In-Memory: ~10 MB
- Disk (JSON): ~5 MB
- Disk (Compressed): ~1.5 MB

### Medium Library (5,000 movies, 10,000 people)
- In-Memory: ~50 MB
- Disk (JSON): ~25 MB
- Disk (Compressed): ~7 MB

### Large Library (20,000 movies, 40,000 people)
- In-Memory: ~200 MB
- Disk (JSON): ~100 MB
- Disk (Compressed): ~30 MB

## Implementation Phases

### Phase 1: In-Memory Only (MVP)
- Build graph on plugin initialization
- Store in memory only
- Rebuild on every server restart
- **Target**: Get working quickly, test with real data

### Phase 2: Add Disk Persistence
- Serialize to JSON on build completion
- Deserialize on startup
- Skip rebuild if cache valid
- **Target**: Faster server startups

### Phase 3: Add Scheduled Task
- Implement IScheduledTask
- Allow manual trigger from UI
- Configurable schedule
- **Target**: Keep cache up-to-date automatically

### Phase 4: Incremental Updates (Future)
- Listen to library change events
- Update graph incrementally
- Avoid full rebuilds
- **Target**: Real-time updates

## Recommended Initial Implementation

For initial development, use **Phase 1 + Phase 2**:

1. **RelationshipGraphService**
   - Build graph in memory
   - Provide query methods
   - Serialize/deserialize to JSON

2. **Storage Location**
   ```csharp
   var dataPath = Path.Combine(
       applicationHost.ApplicationPaths.PluginConfigurationsPath,
       "SixDegrees"
   );
   var cacheFile = Path.Combine(dataPath, "graph-cache.json");
   ```

3. **Initialization Logic**
   ```csharp
   public async Task InitializeAsync()
   {
       if (IsCacheValid())
       {
           logger.Info("Loading graph from cache...");
           await LoadFromDiskAsync();
       }
       else
       {
           logger.Info("Building fresh graph...");
           await BuildGraphAsync();
           await SaveToDiskAsync();
       }
   }
   ```

4. **Add Scheduled Task Later**
   - Can be added in Milestone 10
   - Not critical for initial functionality

## Configuration Options

```csharp
public class PluginConfiguration : BasePluginConfiguration
{
    // How often to rebuild (if using scheduled task)
    public int CacheRefreshIntervalMinutes { get; set; } = 1440; // Daily

    // Enable/disable disk caching
    public bool UseDiskCache { get; set; } = true;

    // Enable/disable compression
    public bool CompressCache { get; set; } = true;

    // Maximum cache age before rebuild
    public int MaxCacheAgeHours { get; set; } = 24;
}
```

## Performance Targets

| Operation | Target | Notes |
|-----------|--------|-------|
| Initial build | < 30s for 10K items | One-time cost |
| Load from disk | < 5s | On startup |
| Save to disk | < 5s | After build |
| Search people | < 50ms | Name search |
| Shortest path | < 100ms | BFS pathfinding |
| N-degree expand | < 200ms | Depends on N |

## Conclusion

**Start with:** In-memory graph + JSON disk persistence (Phase 1+2)

**Benefits:**
- Simple to implement
- Fast queries
- Survives restarts
- Good for most library sizes

**Add later:** Scheduled task (Phase 3) and incremental updates (Phase 4)

This approach balances performance, complexity, and development time effectively.
