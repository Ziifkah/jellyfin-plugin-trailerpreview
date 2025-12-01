# Installation Guide

This guide will help you install the Trailer Preview plugin on your Jellyfin server.

## Prerequisites

- Jellyfin Server **10.8.0** or higher
- .NET 8.0 Runtime (usually included with Jellyfin)

## Installation

### Step 1: Add Plugin Repository

1. Open Jellyfin Dashboard: `http://your-server:8096`
2. Navigate to **Dashboard** → **Plugins** → **Repositories**
3. Click the **+** button to add a repository
4. Enter the following information:
   - **Repository Name**: `Trailer Preview`
   - **Repository URL**: `https://raw.githubusercontent.com/YOUR_USERNAME/jellyfin-plugin-trailerpreview/main/manifest.json`
5. Click **Save**

### Step 2: Install Plugin

1. Go to the **Catalog** tab
2. Find **Trailer Preview** in the list
3. Click **Install**
4. Wait for the installation to complete

### Step 3: Restart Jellyfin

Restart your Jellyfin server for the plugin to load.

#### Windows (Service)
```powershell
net stop JellyfinServer
net start JellyfinServer
```

#### Linux (systemd)
```bash
sudo systemctl restart jellyfin
```

#### Docker
```bash
docker restart jellyfin
```

### Step 4: Verify Installation

1. Go to **Dashboard** → **Plugins**
2. You should see **Trailer Preview** in the list
3. Click on it to access configuration

## Configuration

### Basic Setup

1. Open **Dashboard** → **Plugins** → **Trailer Preview**

2. Recommended initial settings:
   - ✅ Enable Trailer Preview
   - Hover Delay: 800ms
   - Preview Duration: 30000ms (30 seconds)
   - Size Mode: Percentage (35%)
   - ✅ Enable Background Blur
   - ✅ Enable Background Dim
   - ✅ Mute by Default

3. Click **Save**

4. **IMPORTANT**: Fully refresh your browser (Ctrl+F5 or Cmd+Shift+R)

## Verifying It Works

### Quick Test

1. Go to your Movies or TV Shows library
2. Hover your mouse over a movie/show that has a trailer
3. Wait 800ms (configured delay)
4. The trailer should appear in an overlay

### Debug Mode

1. Enable debug logging in the plugin configuration
2. Open browser console (F12)
3. Look for `[TrailerPreview]` messages

Expected messages:
```
[TrailerPreview] Trailer Preview plugin initialized
[TrailerPreview] Configuration: {...}
[TrailerPreview] Platform: {...}
```

## Docker Installations

If you're running Jellyfin in Docker, there may be permission issues for client script injection.

### Option A: Use File Transformation Plugin (Recommended)

1. Install the File Transformation plugin from the Jellyfin catalog
2. Restart Jellyfin
3. Install Trailer Preview normally
4. The plugin will automatically detect File Transformation

### Option B: Map index.html as Volume

Modify your `docker-compose.yml`:

```yaml
version: '3.8'
services:
  jellyfin:
    image: jellyfin/jellyfin:latest
    volumes:
      - /path/to/config:/config
      - /path/to/cache:/cache
      - /path/to/media:/media
      # Add this line for permissions
      - /path/to/config/jellyfin-web/index.html:/jellyfin/jellyfin-web/index.html
    ports:
      - "8096:8096"
    restart: unless-stopped
```

Then restart:
```bash
docker-compose down
docker-compose up -d
```

## Troubleshooting

### Plugin Doesn't Appear in List

**Possible causes:**
- Repository URL was entered incorrectly
- Jellyfin hasn't restarted
- Incompatible Jellyfin version

**Solutions:**
- Verify the repository URL is correct
- Restart Jellyfin server
- Check Jellyfin logs for errors

### No Console Messages

**Cause:** Client script is not injected into index.html

**Solution for Docker:**
1. Install File Transformation plugin
2. Or map index.html as volume (see above)
3. Restart Jellyfin

**Solution for Standard Installation:**
- Check Jellyfin logs
- Verify file permissions on jellyfin-web directory

### Trailers Don't Play

**Verifications:**

1. Does the media have a trailer?
   - Edit the movie/show in Jellyfin
   - Check the "Trailers" section

2. Test the API:
   - Open: `http://your-server:8096/TrailerPreview/Status`
   - Should return JSON with plugin status

3. Check browser console (F12) for errors

### YouTube Trailers Not Working

- Verify YouTube is accessible from your network
- Try different video quality settings in plugin config
- Check if remote trailers are properly configured in Jellyfin

## Performance Optimization

For servers with many users:

```
Max Concurrent Previews: 1
Preload Trailers: false
Disable on Low Bandwidth: true
Preview Duration: 15000ms
```

## Configuration for TV/Remote

```
Hover Delay: 1500ms
Show Close Button: true
Show Mute Button: true
Preview Size: 50%
Enable Background Dim: true
```

## Updating the Plugin

If installed via repository:

1. Go to **Dashboard** → **Plugins**
2. Check for updates
3. Click **Update** if available
4. Restart Jellyfin

## Uninstallation

To uninstall the plugin:

1. Go to **Dashboard** → **Plugins**
2. Click on **Trailer Preview**
3. Click **Uninstall**
4. Restart Jellyfin

## Support

If you encounter issues:

1. **Check logs:**
   ```bash
   # Linux
   tail -f /var/log/jellyfin/jellyfin.log

   # Docker
   docker logs -f jellyfin
   ```

2. **Enable debug logging:**
   - Plugin configuration → Enable Debug Logging
   - Browser console (F12) → Look for messages

3. **Create an issue:**
   - GitHub Issues with logs and configuration details

---

**Enjoy your trailer previews!**
