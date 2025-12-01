# Contributing to Jellyfin Trailer Preview

Thank you for your interest in contributing to the Jellyfin Trailer Preview plugin!

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, JetBrains Rider, or VS Code
- Jellyfin Server 10.8.0 or later (for testing)
- Git

### Building the Plugin

```bash
# Clone the repository
git clone https://github.com/Ziifkah/jellyfin-plugin-trailerpreview.git
cd jellyfin-plugin-trailerpreview

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release
```

### Testing Locally

Copy the built DLL to your Jellyfin plugins directory:

**Windows**: `C:\ProgramData\Jellyfin\Server\plugins\TrailerPreview\`
**Linux**: `/var/lib/jellyfin/plugins/TrailerPreview/`
**Docker**: `/config/plugins/TrailerPreview/`

Then restart Jellyfin.

## Project Structure

```
Jellyfin.Plugin.TrailerPreview/
├── Api/
│   └── TrailerPreviewController.cs    # REST API endpoints
├── Configuration/
│   ├── PluginConfiguration.cs         # Configuration model
│   └── configPage.html                # Admin UI
├── Helpers/
│   └── ScriptBuilder.cs               # Client script generator
├── Models/
│   └── TrailerInfo.cs                 # Data models
└── Plugin.cs                          # Main plugin class
```

## Code Guidelines

### C# Style

- Follow Microsoft C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Use nullable reference types

Example:
```csharp
/// <summary>
/// Retrieves trailer information for a specific item.
/// </summary>
/// <param name="itemId">The unique identifier of the item.</param>
/// <returns>Trailer information or null if not found.</returns>
[HttpGet("TrailerInfo/{itemId}")]
public ActionResult<TrailerInfo> GetTrailerInfo([FromRoute] Guid itemId)
{
    // Implementation
}
```

### JavaScript Style

- Use ES6+ features
- Use `const` and `let` (avoid `var`)
- Add JSDoc comments for functions
- Keep functions small and focused

Example:
```javascript
/**
 * Fetches trailer information from the API
 * @param {string} itemId - The item identifier
 * @returns {Promise<TrailerInfo>} Trailer information
 */
async function fetchTrailerInfo(itemId) {
    // Implementation
}
```

## Adding New Features

### Configuration Options

1. Add property to `PluginConfiguration.cs`:
```csharp
public bool MyNewFeature { get; set; } = true;
```

2. Add to config page `configPage.html`:
```html
<div class="checkboxContainer">
    <label>
        <input type="checkbox" is="emby-checkbox" id="myNewFeature" />
        <span>Enable My New Feature</span>
    </label>
</div>
```

3. Add to client script in `ScriptBuilder.cs`:
```csharp
myNewFeature: {config.MyNewFeature.ToString().ToLower()},
```

4. Update documentation in README.md

### API Endpoints

Add new endpoints in `Api/TrailerPreviewController.cs`:

```csharp
[HttpGet("MyEndpoint")]
[AllowAnonymous]
public ActionResult<MyResponse> GetMyData()
{
    // Implementation
}
```

## Testing

### Manual Testing

Before submitting a pull request:

- [ ] Plugin loads successfully
- [ ] Configuration page works
- [ ] Settings save correctly
- [ ] Trailers play (local and YouTube)
- [ ] No console errors
- [ ] Works on multiple browsers

### Debug Mode

Enable debug logging in plugin config, then check browser console (F12) for `[TrailerPreview]` messages.

## Pull Request Process

1. Create a feature branch (`feature/amazing-feature`)
2. Make your changes
3. Test thoroughly
4. Update documentation if needed
5. Submit pull request with clear description

### PR Description Template

```markdown
## Summary
Brief description of changes

## Changes
- Added X feature
- Fixed Y bug
- Updated Z documentation

## Testing
- Tested on Web client
- Tested on Desktop client
- No console errors
```

## Performance

- Minimize DOM manipulations
- Use event delegation
- Cache API responses
- Optimize animations (use CSS transforms)
- Lazy load resources

## Security

- Validate all inputs
- Sanitize HTML to prevent XSS
- Use HTTPS for remote resources
- Handle errors gracefully
- Don't expose sensitive information

## Code Review

Reviewers check for:
- Code quality and style
- Performance implications
- Security concerns
- Documentation completeness
- Breaking changes

## Questions?

Open a GitHub issue or discussion if you need help.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing!
