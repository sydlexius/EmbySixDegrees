# Semantic Versioning

This project uses [Semantic Versioning 2.0.0](https://semver.org/) (MAJOR.MINOR.PATCH).

## Version Format

```
v1.2.3
 │ │ │
 │ │ └── PATCH: Bug fixes, minor changes
 │ └──── MINOR: New features, backwards-compatible
 └────── MAJOR: Breaking changes
```

## Automated Versioning

This project uses [GitVersion](https://gitversion.net/) for automatic version calculation based on Git history.

### How It Works

1. **Default Behavior**: Each commit increments the patch version
2. **Explicit Control**: Use commit message tags to control versioning

### Commit Message Tags

Control the version bump with these tags in your commit message:

```bash
# Patch version bump (1.0.0 → 1.0.1)
git commit -m "Fix pathfinding bug +semver:patch"
git commit -m "Fix null reference in search +semver:fix"

# Minor version bump (1.0.0 → 1.1.0)
git commit -m "Add depth slider control +semver:minor"
git commit -m "Implement graph caching +semver:feature"

# Major version bump (1.0.0 → 2.0.0)
git commit -m "Refactor API endpoints (breaking) +semver:major"
git commit -m "Change graph data format +semver:breaking"

# No version bump
git commit -m "Update documentation +semver:none"
git commit -m "Fix typo in README +semver:skip"
```

## Branch Strategy

- **main**: Production releases (v1.0.0, v1.1.0, etc.)
- **develop**: Alpha releases (v1.1.0-alpha.1, etc.)
- **feature/***: Beta releases (v1.1.0-beta.1, etc.)

## Release Process

### Creating a Release

1. **Ensure all changes are committed**
   ```bash
   git status
   ```

2. **Push to main branch**
   ```bash
   git push origin main
   ```

3. **Create and push a version tag**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

4. **GitHub Actions will:**
   - Build the plugin with the version number
   - Create a GitHub release
   - Attach the compiled DLL as an artifact

### Pre-releases

For alpha/beta versions, push to develop or feature branches:

```bash
git checkout develop
git commit -m "Add new feature +semver:minor"
git push origin develop
```

This creates a version like `v1.1.0-alpha.1`.

## Version Numbers in Code

The project file uses GitVersion to inject version numbers:

```xml
<PropertyGroup>
  <AssemblyVersion>$(GitVersion_AssemblySemVer)</AssemblyVersion>
  <FileVersion>$(GitVersion_AssemblySemVer)</FileVersion>
  <InformationalVersion>$(GitVersion_InformationalVersion)</InformationalVersion>
</PropertyGroup>
```

## Current Version

Check the current version:

```bash
# Local
dotnet gitversion

# GitHub
Check the latest tag: https://github.com/sydlexius/EmbySixDegrees/tags
```

## Version History

### v0.1.0 (Planned)
- Initial project setup
- Documentation and architecture
- Basic plugin structure

### v1.0.0 (Planned)
- Complete relationship graph service
- REST API endpoints
- D3.js visualization
- Search interface
- Interactive features

### v1.1.0 (Planned)
- Configuration settings page
- Scheduled task for cache refresh
- Performance optimizations

### v2.0.0 (Future)
- Incremental graph updates
- Advanced filters
- Export functionality
- Graph statistics

## Examples

### Example Workflow

```bash
# Feature development
git checkout -b feature/search-interface
git commit -m "Add person search with autocomplete +semver:minor"
git push origin feature/search-interface

# Create PR, merge to develop
# Test alpha version

# When ready for release
git checkout main
git merge develop
git tag v1.1.0
git push origin main --tags

# GitHub Actions builds and releases v1.1.0
```

### Checking Version in Plugin

The plugin version can be accessed at runtime:

```csharp
var version = Assembly.GetExecutingAssembly()
    .GetName()
    .Version
    .ToString();

logger.Info($"Six Degrees Plugin v{version} loaded");
```

## References

- [Semantic Versioning Specification](https://semver.org/)
- [GitVersion Documentation](https://gitversion.net/docs/)
- [Conventional Commits](https://www.conventionalcommits.org/)
