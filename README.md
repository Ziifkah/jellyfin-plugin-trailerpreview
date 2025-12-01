# Jellyfin Trailer Preview Plugin

A modern, Netflix-style trailer preview plugin for Jellyfin that displays movie and TV show trailers when hovering over items.

![License](https://img.shields.io/github/license/Ziifkah/jellyfin-plugin-trailerpreview)
![Release](https://img.shields.io/github/v/release/Ziifkah/jellyfin-plugin-trailerpreview)

## Features

### Core Features
- **Netflix-Style Hover Previews**: Display trailers in a beautiful overlay when hovering over movies and TV shows
- **Multi-Platform Support**: Works seamlessly on Web, Desktop, Mobile, and TV clients
- **Local & Remote Trailers**: Supports both local trailer files and YouTube trailers
- **Smooth Animations**: Elegant fade-in/fade-out effects with customizable durations

### Customization Options
- **Timing Controls**: Configure hover delay, preview duration, and animation speeds
- **Position Settings**: Center or custom positioning with X/Y offsets
- **Size Options**: Percentage-based or manual pixel sizing
- **Visual Effects**: Background blur, dimming, opacity, and border radius controls
- **Audio Settings**: Enable/disable audio, volume control, mute by default
- **UI Elements**: Toggle progress bar, close button, mute button, and title display

### Advanced Features
- **Platform Detection**: Automatically adapts to the client platform
- **Trailer Caching**: Smart caching for improved performance
- **Concurrent Previews**: Control maximum simultaneous previews
- **Bandwidth Detection**: Optional automatic disable on slow connections
- **Debug Logging**: Comprehensive logging for troubleshooting

## Installation

### 1. Add Plugin Repository

1. Open your Jellyfin dashboard
2. Navigate to **Dashboard** → **Plugins** → **Repositories**
3. Click the **+** button to add a new repository
4. Enter the following information:
   - **Repository Name**: `Trailer Preview`
   - **Repository URL**: `https://raw.githubusercontent.com/Ziifkah/jellyfin-plugin-trailerpreview/main/manifest.json`
5. Click **Save**

### 2. Install Plugin

1. Go to the **Catalog** tab in Plugins
2. Find **Trailer Preview** in the list
3. Click **Install**
4. Restart your Jellyfin server

### 3. Configure

1. Go to **Dashboard** → **Plugins** → **Trailer Preview**
2. Configure settings as desired
3. Click **Save**
4. Refresh your browser (Ctrl+F5)

That's it! Now hover over any movie or TV show that has a trailer.

## Configuration

### Recommended Settings

#### For Best Performance
```
Hover Delay: 800ms
Preview Duration: 30000ms (30 seconds)
Size Mode: Percentage (35%)
Enable Background Blur: Yes
Enable Background Dim: Yes
Mute by Default: Yes
Preload Trailers: Yes
```

#### For TV/Remote Control Use
```
Hover Delay: 1200ms
Size Mode: Percentage (50%)
Show Close Button: Yes
Show Mute Button: Yes
Enable on TV: Yes
```

#### For Mobile Devices
```
Hover Delay: 1000ms
Size Mode: Percentage (90%)
Enable Background Dim: Yes
Enable on Mobile: Yes
Disable on Low Bandwidth: Yes
```

## Usage

### Basic Usage

1. Navigate to your Movies or TV Shows library
2. Hover your mouse over any item
3. Wait for the configured hover delay (default: 800ms)
4. The trailer will appear in an elegant overlay
5. Press ESC or click outside to dismiss

### Keyboard Shortcuts

- **ESC**: Close the currently playing trailer preview
- **Click Overlay**: Click outside the video to close

### Controls

- **Mute Button**: Toggle audio on/off (for local trailers)
- **Close Button**: Manually close the preview
- **Progress Bar**: Visual indicator of playback progress

## Platform Compatibility

| Platform | Status | Notes |
|----------|--------|-------|
| Web Client | ✅ Fully Supported | Chrome, Firefox, Edge, Safari |
| Jellyfin Desktop (Windows) | ✅ Fully Supported | Electron-based client |
| Jellyfin Desktop (Linux) | ✅ Fully Supported | Electron-based client |
| Jellyfin Desktop (macOS) | ✅ Fully Supported | Electron-based client |
| Android App | ⚠️ Limited | Hover detection may not work on touch |
| iOS App | ⚠️ Limited | Hover detection may not work on touch |
| TV Apps | ✅ Supported | Remote control navigation |

## Docker Considerations

If you're running Jellyfin in Docker and the plugin can't inject the client script, you have two options:

### Option 1: Use File Transformation Plugin (Recommended)

Install the [File Transformation plugin](https://github.com/jellyfin/jellyfin-plugin-file-transformation) which allows runtime file modifications without permission issues.

### Option 2: Volume Mapping

Add the index.html as a volume in your docker-compose.yml:

```yaml
services:
  jellyfin:
    volumes:
      - /path/to/config/jellyfin-web/index.html:/jellyfin/jellyfin-web/index.html
```

This gives the plugin write permissions to inject the client script.

## Troubleshooting

### Trailers Not Showing

1. **Check if trailers are available**:
   - Ensure your movies have local trailers or remote trailer URLs
   - In Jellyfin, edit a movie and check the "Trailers" section

2. **Enable debug logging**:
   - Go to plugin settings
   - Enable "Debug Logging"
   - Open browser console (F12) to see detailed logs

3. **Verify plugin is loaded**:
   - Open browser console
   - Look for `[TrailerPreview] Trailer Preview plugin initialized`

### Script Not Injecting (Docker)

If you see no console messages:

1. Check Jellyfin server logs for errors
2. Install File Transformation plugin
3. Or add volume mapping (see Docker section)
4. Restart Jellyfin server

### YouTube Trailers Not Playing

1. Check if YouTube is accessible from your network
2. Try different video quality settings
3. Ensure remote trailers are configured in Jellyfin

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## Development

For developers who want to contribute or build from source, see [CONTRIBUTING.md](CONTRIBUTING.md) for detailed instructions.

## Changelog

### Version 1.0.0 (2025-12-01)

- Initial release
- Netflix-style hover trailer previews
- Multi-platform support (Web, Desktop, Mobile, TV)
- 40+ configuration options
- Local and remote trailer support
- Background blur and dim effects
- Smart caching and performance optimizations
- Comprehensive error handling and logging

## License

This plugin is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Acknowledgments

- Inspired by Netflix's trailer preview feature
- Thanks to the Jellyfin team for the excellent media server platform
- Built with reference to existing Jellyfin plugins

## Support

- **Issues**: [GitHub Issues](https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/discussions)
- **Jellyfin Forum**: [Plugin Development](https://forum.jellyfin.org/f-plugin-development)

---

**Made with ❤️ for the Jellyfin community**
