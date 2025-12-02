# Trailer Preview for Jellyfin v2.0.0

A complete rewrite of the Trailer Preview plugin, providing Netflix-style trailer previews when hovering over movies and TV shows in Jellyfin.

## What's New in v2.0.0

- **Complete Rewrite**: Built on HoverTrailer's proven architecture for stability
- **Netflix-Style Overlay**: Centered, fullscreen overlay with background dimming
- **Comprehensive Configuration**: 20+ customizable options
- **Fixed Critical Bugs**: Resolved JavaScript decimal formatting issues
- **Improved Error Handling**: Defensive programming, never crashes
- **Multi-Platform Support**: Web, Desktop, Mobile, and TV clients
- **Better Performance**: Optimized script injection and event handling

## Features

### General Settings
- Enable/disable trailer previews
- Debug logging for troubleshooting

### Timing Controls
- Hover delay (default: 800ms)
- Preview duration (default: 30s, 0 = unlimited)
- Fade in/out animations (customizable)

### Display Options
- Preview size (width/height as viewport percentage)
- Position (center, top, bottom)
- Background dimming with adjustable opacity
- Title overlay

### Audio Controls
- Enable/disable audio
- Adjustable volume (default: 30%)

### Platform Support
- Selective enable/disable for Web, Desktop, Mobile, TV

## Installation

### From Jellyfin Dashboard

1. Go to **Dashboard** → **Plugins** → **Repositories**
2. Add repository: `https://raw.githubusercontent.com/Ziifkah/jellyfin-plugin-trailerpreview/main/manifest.json`
3. Go to **Catalog** and install "Trailer Preview"
4. Restart Jellyfin

### Manual Installation

1. Download `jellyfin-plugin-trailerpreview_2.0.0.zip` from releases
2. Extract to `{jellyfin-config}/plugins/Trailer Preview/`
3. Restart Jellyfin

## Configuration

Navigate to **Dashboard** → **Plugins** → **Trailer Preview** to configure all options.

## Requirements

- Jellyfin 10.10.0 or higher
- .NET 8.0 runtime
- Movies/TV shows with trailer metadata (YouTube URLs or local files)

## Technical Details

### Architecture

- **Plugin**: `Plugin.cs` - Handles initialization and script injection
- **Configuration**: `PluginConfiguration.cs` - All configuration options
- **API Controller**: `TrailerPreviewController.cs` - Serves client script and trailer data
- **Client Script**: Generated dynamically with configuration injected

### Script Injection

The plugin uses a dual approach:
1. **File Transformation Plugin** (preferred): Registers with FileTransformation plugin if available
2. **Direct Injection** (fallback): Modifies index.html directly

### How It Works

1. Plugin injects JavaScript into Jellyfin's web client
2. JavaScript observes DOM for movie/TV show cards
3. On hover (after delay), fetches trailer info from API
4. Creates Netflix-style overlay with video player
5. Removes overlay on click or after duration

## Troubleshooting

### Plugin doesn't appear in catalog
- Verify repository URL is correct
- Check Jellyfin version (must be 10.10.0+)
- Restart Jellyfin server

### Previews don't show
- Enable debug logging in plugin settings
- Check browser console (F12) for errors
- Verify movies have trailer metadata
- Check platform support settings

### Configuration page blank
- Clear browser cache
- Hard refresh (Ctrl+Shift+R)
- Check browser console for JavaScript errors

## Credits

- Based on [HoverTrailer](https://github.com/Fovty/HoverTrailer) by Fovty
- Developed with Claude Code
- Maintained by Ziifkah

## License

MIT License - See LICENSE file for details

## Changelog

### v2.0.0 (2025-12-02)
- Complete rewrite based on HoverTrailer architecture
- Netflix-style overlay implementation
- Fixed JavaScript decimal formatting bugs (CultureInfo.InvariantCulture)
- Comprehensive configuration options (20+ settings)
- Improved error handling and stability
- Multi-platform support
- Better performance and resource management

### v1.0.0 (2025-12-01)
- Initial release
- Basic hover preview functionality
