# Installation Guide - TrailerPreview v2.0.0

## Quick Start

### Option 1: Install from Repository (Recommended)

1. Open Jellyfin Dashboard
2. Go to **Plugins** → **Repositories**
3. Click **Add** and enter:
   - Repository Name: `TrailerPreview`
   - Repository URL: `https://raw.githubusercontent.com/Ziifkah/jellyfin-plugin-trailerpreview/main/manifest.json`
4. Go to **Plugins** → **Catalog**
5. Find "Trailer Preview" and click **Install**
6. Restart Jellyfin server

### Option 2: Manual Installation

1. Download `jellyfin-plugin-trailerpreview_2.0.0.zip`
2. Navigate to your Jellyfin plugin directory:
   - Windows: `C:\ProgramData\Jellyfin\Server\plugins\`
   - Linux: `/var/lib/jellyfin/plugins/`
   - Docker: `/config/plugins/`
3. Create directory: `Trailer Preview`
4. Extract ZIP contents into this directory
5. Restart Jellyfin server

## Verification

1. Go to **Dashboard** → **Plugins**
2. You should see "Trailer Preview v2.0.0" in the list
3. Click on it to access configuration page
4. Enable the plugin and configure options
5. Save settings

## Testing

1. Navigate to your movie library
2. Hover over a movie card that has trailer metadata
3. After the configured delay (default: 800ms), the preview should appear
4. Click anywhere to close the preview

## Troubleshooting

### Plugin doesn't appear
- Check Jellyfin logs: **Dashboard** → **Logs**
- Verify file structure:
  ```
  plugins/
    Trailer Preview/
      Jellyfin.Plugin.TrailerPreview.dll
      meta.json
  ```
- Restart Jellyfin again

### Previews don't show
1. Enable **Debug Logging** in plugin settings
2. Open browser console (F12)
3. Hover over a movie
4. Check for errors or debug messages
5. Common issues:
   - No trailer metadata on items
   - Platform support disabled for your client
   - JavaScript errors (check console)

### Configuration page blank
1. Clear browser cache
2. Hard refresh: `Ctrl + Shift + R` (Windows) or `Cmd + Shift + R` (Mac)
3. Try different browser
4. Check browser console for errors

## GitHub Release Setup

To publish v2.0.0 to GitHub:

1. **Navigate to repository**
   ```bash
   cd C:\Users\Dorian
   git clone https://github.com/Ziifkah/jellyfin-plugin-trailerpreview.git
   cd jellyfin-plugin-trailerpreview
   ```

2. **Copy v2 files**
   ```bash
   # Copy source code
   xcopy /E /I C:\Users\Dorian\TrailerPreviewV2\Jellyfin.Plugin.TrailerPreview .\Jellyfin.Plugin.TrailerPreview

   # Copy manifest
   copy C:\Users\Dorian\TrailerPreviewV2\manifest.json .

   # Copy documentation
   copy C:\Users\Dorian\TrailerPreviewV2\README.md .
   ```

3. **Commit changes**
   ```bash
   git add .
   git commit -m "Release v2.0.0 - Complete rewrite with Netflix-style overlay

   - Complete rewrite based on HoverTrailer architecture
   - Netflix-style overlay with comprehensive configuration
   - Fixed JavaScript decimal formatting bugs
   - Improved stability and error handling
   - Multi-platform support
   - 20+ configuration options

   🤖 Generated with [Claude Code](https://claude.com/claude-code)

   Co-Authored-By: Claude <noreply@anthropic.com>"
   ```

4. **Push to GitHub**
   ```bash
   git push origin main
   ```

5. **Create GitHub Release**
   - Go to: https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/releases
   - Click "Draft a new release"
   - Tag: `v2.0.0`
   - Title: `v2.0.0 - Complete Rewrite`
   - Description: Copy from README.md changelog section
   - Upload: `jellyfin-plugin-trailerpreview_2.0.0.zip`
   - Click "Publish release"

6. **Verify manifest URLs**
   - The manifest.json sourceUrl should now be accessible
   - Test installation from repository in Jellyfin

## Next Steps

1. Test the plugin thoroughly on your Jellyfin server
2. Report any issues on GitHub
3. Configure settings to your preference
4. Enjoy Netflix-style trailer previews!

## Support

- GitHub Issues: https://github.com/Ziifkah/jellyfin-plugin-trailerpreview/issues
- Check logs for detailed error messages
- Enable debug logging for troubleshooting
