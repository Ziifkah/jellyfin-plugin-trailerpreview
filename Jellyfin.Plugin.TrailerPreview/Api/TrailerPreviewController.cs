using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Plugin.TrailerPreview.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TrailerPreview.Api;

[ApiController]
[Authorize]
[Route("TrailerPreview")]
public class TrailerPreviewController : ControllerBase
{
    private readonly ILogger<TrailerPreviewController> _logger;
    private readonly ILibraryManager _libraryManager;

    public TrailerPreviewController(
        ILogger<TrailerPreviewController> logger,
        ILibraryManager libraryManager)
    {
        _logger = logger;
        _libraryManager = libraryManager;
    }

    [HttpGet("ClientScript")]
    [AllowAnonymous]
    [Produces("application/javascript")]
    public ActionResult<string> GetClientScript()
    {
        try
        {
            var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

            var script = BuildClientScript(config);
            return Content(script, "application/javascript", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating client script");
            return Content("console.error('TrailerPreview: Failed to load client script');", "application/javascript");
        }
    }

    [HttpGet("Status")]
    public ActionResult<object> GetStatus()
    {
        try
        {
            var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
            return Ok(new
            {
                enabled = config.EnableTrailerPreview,
                debugLogging = config.EnableDebugLogging,
                version = Plugin.Instance?.Version.ToString() ?? "2.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("Trailer/{itemId}")]
    [AllowAnonymous]
    public ActionResult<object> GetTrailerInfo(Guid itemId)
    {
        try
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound(new { error = "Item not found" });
            }

            var trailers = new List<object>();

            // Get remote trailers (YouTube and others)
            if (item.RemoteTrailers != null && item.RemoteTrailers.Count > 0)
            {
                foreach (var remoteTrailer in item.RemoteTrailers)
                {
                    var url = remoteTrailer.Url;
                    if (!string.IsNullOrEmpty(url))
                    {
                        trailers.Add(new
                        {
                            type = "youtube",
                            name = remoteTrailer.Name ?? "Trailer",
                            url = url
                        });
                    }
                }
            }

            return Ok(new
            {
                itemId = itemId.ToString(),
                itemName = item.Name,
                trailers = trailers,
                hasTrailers = trailers.Count > 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trailer info for item {ItemId}", itemId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private string BuildClientScript(PluginConfiguration config)
    {
        var sb = new StringBuilder();

        sb.AppendLine("(function() {");
        sb.AppendLine("  'use strict';");
        sb.AppendLine();

        // Configuration
        sb.AppendLine("  const CONFIG = {");
        sb.AppendLine($"    enabled: {config.EnableTrailerPreview.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    debugLogging: {config.EnableDebugLogging.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    hoverDelayMs: {config.HoverDelayMs},");
        sb.AppendLine($"    previewDurationMs: {config.PreviewDurationMs},");
        sb.AppendLine($"    fadeInDurationMs: {config.FadeInDurationMs},");
        sb.AppendLine($"    fadeOutDurationMs: {config.FadeOutDurationMs},");
        sb.AppendLine($"    previewWidthPercent: {config.PreviewWidthPercent},");
        sb.AppendLine($"    previewHeightPercent: {config.PreviewHeightPercent},");
        sb.AppendLine($"    previewPosition: '{config.PreviewPosition}',");
        sb.AppendLine($"    enableBackgroundDim: {config.EnableBackgroundDim.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    backgroundDimOpacity: {config.BackgroundDimOpacity.ToString(CultureInfo.InvariantCulture)},");
        sb.AppendLine($"    previewOpacity: {config.PreviewOpacity.ToString(CultureInfo.InvariantCulture)},");
        sb.AppendLine($"    showTitle: {config.ShowTitle.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    enableAudio: {config.EnableAudio.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    audioVolume: {config.AudioVolume},");
        sb.AppendLine($"    enableOnWeb: {config.EnableOnWeb.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    enableOnDesktop: {config.EnableOnDesktop.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    enableOnMobile: {config.EnableOnMobile.ToString().ToLowerInvariant()},");
        sb.AppendLine($"    enableOnTV: {config.EnableOnTV.ToString().ToLowerInvariant()}");
        sb.AppendLine("  };");
        sb.AppendLine();

        // Debug logger
        sb.AppendLine("  function debugLog(...args) {");
        sb.AppendLine("    if (CONFIG.debugLogging) {");
        sb.AppendLine("      console.log('[TrailerPreview]', ...args);");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Check if enabled
        sb.AppendLine("  if (!CONFIG.enabled) {");
        sb.AppendLine("    debugLog('Plugin is disabled');");
        sb.AppendLine("    return;");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Platform detection
        sb.AppendLine("  function detectPlatform() {");
        sb.AppendLine("    const ua = navigator.userAgent.toLowerCase();");
        sb.AppendLine("    if (ua.includes('mobile') || ua.includes('android') || ua.includes('iphone')) return 'mobile';");
        sb.AppendLine("    if (ua.includes('tv') || ua.includes('smarttv')) return 'tv';");
        sb.AppendLine("    if (window.NativeShell) return 'desktop';");
        sb.AppendLine("    return 'web';");
        sb.AppendLine("  }");
        sb.AppendLine();
        sb.AppendLine("  const platform = detectPlatform();");
        sb.AppendLine("  debugLog('Platform detected:', platform);");
        sb.AppendLine();

        // Check platform support
        sb.AppendLine("  const platformEnabled = {");
        sb.AppendLine("    web: CONFIG.enableOnWeb,");
        sb.AppendLine("    desktop: CONFIG.enableOnDesktop,");
        sb.AppendLine("    mobile: CONFIG.enableOnMobile,");
        sb.AppendLine("    tv: CONFIG.enableOnTV");
        sb.AppendLine("  };");
        sb.AppendLine();
        sb.AppendLine("  if (!platformEnabled[platform]) {");
        sb.AppendLine("    debugLog('Plugin disabled for platform:', platform);");
        sb.AppendLine("    return;");
        sb.AppendLine("  }");
        sb.AppendLine();

        // State management
        sb.AppendLine("  let currentHoverTimeout = null;");
        sb.AppendLine("  let currentPreviewTimeout = null;");
        sb.AppendLine("  let currentPreview = null;");
        sb.AppendLine("  let isPreviewShowing = false;");
        sb.AppendLine();

        // Get API key
        sb.AppendLine("  function getApiKey() {");
        sb.AppendLine("    try {");
        sb.AppendLine("      return ApiClient?.accessToken() || localStorage.getItem('jellyfin_credentials')?.split(',')[0] || '';");
        sb.AppendLine("    } catch (e) {");
        sb.AppendLine("      return '';");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Extract YouTube ID
        sb.AppendLine("  function extractYouTubeId(url) {");
        sb.AppendLine("    if (!url) return null;");
        sb.AppendLine("    const patterns = [");
        sb.AppendLine("      /(?:youtube\\.com\\/watch\\?v=|youtu\\.be\\/)([^&\\s]+)/,");
        sb.AppendLine("      /youtube\\.com\\/embed\\/([^?&\\s]+)/");
        sb.AppendLine("    ];");
        sb.AppendLine("    for (const pattern of patterns) {");
        sb.AppendLine("      const match = url.match(pattern);");
        sb.AppendLine("      if (match) return match[1];");
        sb.AppendLine("    }");
        sb.AppendLine("    return null;");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Create preview overlay
        sb.AppendLine("  function createPreviewOverlay(trailerData, itemName) {");
        sb.AppendLine("    debugLog('Creating preview overlay for:', itemName);");
        sb.AppendLine();
        sb.AppendLine("    const overlay = document.createElement('div');");
        sb.AppendLine("    overlay.id = 'trailer-preview-overlay';");
        sb.AppendLine("    overlay.style.cssText = `");
        sb.AppendLine("      position: fixed;");
        sb.AppendLine("      top: 0;");
        sb.AppendLine("      left: 0;");
        sb.AppendLine("      width: 100vw;");
        sb.AppendLine("      height: 100vh;");
        sb.AppendLine("      z-index: 9999;");
        sb.AppendLine("      display: flex;");
        sb.AppendLine("      align-items: center;");
        sb.AppendLine("      justify-content: center;");
        sb.AppendLine("      pointer-events: auto;");
        sb.AppendLine("      opacity: 0;");
        sb.AppendLine($"      transition: opacity {config.FadeInDurationMs}ms ease-in-out;");
        sb.AppendLine("    `;");
        sb.AppendLine();

        // Background dim
        sb.AppendLine("    if (CONFIG.enableBackgroundDim) {");
        sb.AppendLine("      const background = document.createElement('div');");
        sb.AppendLine("      background.style.cssText = `");
        sb.AppendLine("        position: absolute;");
        sb.AppendLine("        top: 0;");
        sb.AppendLine("        left: 0;");
        sb.AppendLine("        width: 100%;");
        sb.AppendLine("        height: 100%;");
        sb.AppendLine("        background: rgba(0, 0, 0, ${CONFIG.backgroundDimOpacity});");
        sb.AppendLine("        backdrop-filter: blur(5px);");
        sb.AppendLine("      `;");
        sb.AppendLine("      overlay.appendChild(background);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Preview container
        sb.AppendLine("    const container = document.createElement('div');");
        sb.AppendLine("    container.style.cssText = `");
        sb.AppendLine("      position: relative;");
        sb.AppendLine("      width: ${CONFIG.previewWidthPercent}vw;");
        sb.AppendLine("      height: ${CONFIG.previewHeightPercent}vh;");
        sb.AppendLine("      max-width: 1200px;");
        sb.AppendLine("      max-height: 675px;");
        sb.AppendLine("      background: #000;");
        sb.AppendLine("      border-radius: 8px;");
        sb.AppendLine("      overflow: hidden;");
        sb.AppendLine("      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.8);");
        sb.AppendLine("      opacity: ${CONFIG.previewOpacity};");
        sb.AppendLine("    `;");
        sb.AppendLine();

        // Video element
        sb.AppendLine("    const trailer = trailerData.trailers[0];");
        sb.AppendLine("    let videoElement;");
        sb.AppendLine();
        sb.AppendLine("    if (trailer.type === 'youtube') {");
        sb.AppendLine("      const youtubeId = extractYouTubeId(trailer.url);");
        sb.AppendLine("      if (youtubeId) {");
        sb.AppendLine("        videoElement = document.createElement('iframe');");
        sb.AppendLine("        // Note: Browsers may block autoplay with audio. YouTube will start muted by default for autoplay.");
        sb.AppendLine("        const muteParam = CONFIG.enableAudio ? '0' : '1';");
        sb.AppendLine("        videoElement.src = `https://www.youtube-nocookie.com/embed/${youtubeId}?autoplay=1&mute=${muteParam}&controls=0&loop=1&playlist=${youtubeId}&playsinline=1&rel=0&modestbranding=1&enablejsapi=1&origin=${window.location.origin}`;");
        sb.AppendLine("        videoElement.allow = 'accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture';");
        sb.AppendLine("        videoElement.setAttribute('allowfullscreen', '');");
        sb.AppendLine("        videoElement.setAttribute('referrerpolicy', 'origin');");
        sb.AppendLine("        videoElement.style.cssText = 'width: 100%; height: 100%; border: none;';");
        sb.AppendLine("        debugLog(`YouTube iframe created with mute=${muteParam}, audio enabled: ${CONFIG.enableAudio}`);");
        sb.AppendLine("      }");
        sb.AppendLine("    } else if (trailer.type === 'local') {");
        sb.AppendLine("      videoElement = document.createElement('video');");
        sb.AppendLine("      videoElement.src = trailer.url.replace('{API_KEY}', getApiKey());");
        sb.AppendLine("      videoElement.autoplay = true;");
        sb.AppendLine("      videoElement.muted = !CONFIG.enableAudio;");
        sb.AppendLine("      if (CONFIG.enableAudio) {");
        sb.AppendLine("        videoElement.volume = CONFIG.audioVolume / 100;");
        sb.AppendLine("      }");
        sb.AppendLine("      videoElement.style.cssText = 'width: 100%; height: 100%; object-fit: contain;';");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    if (videoElement) {");
        sb.AppendLine("      container.appendChild(videoElement);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Title overlay
        sb.AppendLine("    if (CONFIG.showTitle) {");
        sb.AppendLine("      const title = document.createElement('div');");
        sb.AppendLine("      title.textContent = itemName;");
        sb.AppendLine("      title.style.cssText = `");
        sb.AppendLine("        position: absolute;");
        sb.AppendLine("        bottom: 20px;");
        sb.AppendLine("        left: 20px;");
        sb.AppendLine("        right: 20px;");
        sb.AppendLine("        color: white;");
        sb.AppendLine("        font-size: 24px;");
        sb.AppendLine("        font-weight: bold;");
        sb.AppendLine("        text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);");
        sb.AppendLine("        z-index: 10;");
        sb.AppendLine("      `;");
        sb.AppendLine("      container.appendChild(title);");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    overlay.appendChild(container);");
        sb.AppendLine();

        // Click to close
        sb.AppendLine("    overlay.addEventListener('click', (e) => {");
        sb.AppendLine("      if (e.target === overlay || e.target.parentElement === overlay) {");
        sb.AppendLine("        removePreview();");
        sb.AppendLine("      }");
        sb.AppendLine("    });");
        sb.AppendLine();

        sb.AppendLine("    document.body.appendChild(overlay);");
        sb.AppendLine("    setTimeout(() => overlay.style.opacity = '1', 10);");
        sb.AppendLine();
        sb.AppendLine("    return overlay;");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Remove preview
        sb.AppendLine("  function removePreview() {");
        sb.AppendLine("    if (currentPreview) {");
        sb.AppendLine("      debugLog('Removing preview');");
        sb.AppendLine("      // Stop all videos and iframes before removing");
        sb.AppendLine("      const videos = currentPreview.querySelectorAll('video');");
        sb.AppendLine("      videos.forEach(video => {");
        sb.AppendLine("        video.pause();");
        sb.AppendLine("        video.src = '';");
        sb.AppendLine("        video.load();");
        sb.AppendLine("      });");
        sb.AppendLine("      const iframes = currentPreview.querySelectorAll('iframe');");
        sb.AppendLine("      iframes.forEach(iframe => {");
        sb.AppendLine("        iframe.src = 'about:blank';");
        sb.AppendLine("      });");
        sb.AppendLine($"      currentPreview.style.transition = 'opacity {config.FadeOutDurationMs}ms ease-in-out';");
        sb.AppendLine("      currentPreview.style.opacity = '0';");
        sb.AppendLine($"      setTimeout(() => {{");
        sb.AppendLine("        if (currentPreview && currentPreview.parentElement) {");
        sb.AppendLine("          currentPreview.remove();");
        sb.AppendLine("        }");
        sb.AppendLine("        currentPreview = null;");
        sb.AppendLine("        isPreviewShowing = false;");
        sb.AppendLine($"      }}, {config.FadeOutDurationMs});");
        sb.AppendLine("    }");
        sb.AppendLine("    if (currentHoverTimeout) {");
        sb.AppendLine("      clearTimeout(currentHoverTimeout);");
        sb.AppendLine("      currentHoverTimeout = null;");
        sb.AppendLine("    }");
        sb.AppendLine("    if (currentPreviewTimeout) {");
        sb.AppendLine("      clearTimeout(currentPreviewTimeout);");
        sb.AppendLine("      currentPreviewTimeout = null;");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Show preview
        sb.AppendLine("  async function showPreview(itemId, itemName) {");
        sb.AppendLine("    try {");
        sb.AppendLine("      // Remove any existing preview first to avoid overlaps");
        sb.AppendLine("      if (currentPreview) {");
        sb.AppendLine("        removePreview();");
        sb.AppendLine("        await new Promise(resolve => setTimeout(resolve, 100));");
        sb.AppendLine("      }");
        sb.AppendLine("      debugLog('Fetching trailer for:', itemName, itemId);");
        sb.AppendLine("      const response = await fetch(`/TrailerPreview/Trailer/${itemId}`);");
        sb.AppendLine("      if (!response.ok) {");
        sb.AppendLine("        debugLog('Failed to fetch trailer info');");
        sb.AppendLine("        return;");
        sb.AppendLine("      }");
        sb.AppendLine("      const data = await response.json();");
        sb.AppendLine("      if (!data.hasTrailers) {");
        sb.AppendLine("        debugLog('No trailers available for:', itemName);");
        sb.AppendLine("        return;");
        sb.AppendLine("      }");
        sb.AppendLine("      currentPreview = createPreviewOverlay(data, itemName);");
        sb.AppendLine("      isPreviewShowing = true;");
        sb.AppendLine("      if (CONFIG.previewDurationMs > 0) {");
        sb.AppendLine("        currentPreviewTimeout = setTimeout(removePreview, CONFIG.previewDurationMs);");
        sb.AppendLine("      }");
        sb.AppendLine("    } catch (error) {");
        sb.AppendLine("      debugLog('Error showing preview:', error);");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Handle card hover
        sb.AppendLine("  function handleCardHover(card) {");
        sb.AppendLine("    if (isPreviewShowing) return;");
        sb.AppendLine();
        sb.AppendLine("    const itemId = card.getAttribute('data-id');");
        sb.AppendLine("    const itemType = card.getAttribute('data-type');");
        sb.AppendLine("    if (!itemId || (itemType !== 'Movie' && itemType !== 'Series')) {");
        sb.AppendLine("      return;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    const itemName = card.querySelector('.cardText')?.textContent || 'Unknown';");
        sb.AppendLine();
        sb.AppendLine("    if (currentHoverTimeout) {");
        sb.AppendLine("      clearTimeout(currentHoverTimeout);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    currentHoverTimeout = setTimeout(() => {");
        sb.AppendLine("      showPreview(itemId, itemName);");
        sb.AppendLine("    }, CONFIG.hoverDelayMs);");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Handle card leave
        sb.AppendLine("  function handleCardLeave() {");
        sb.AppendLine("    if (currentHoverTimeout) {");
        sb.AppendLine("      clearTimeout(currentHoverTimeout);");
        sb.AppendLine("      currentHoverTimeout = null;");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Attach listeners
        sb.AppendLine("  function attachListeners() {");
        sb.AppendLine("    const cards = document.querySelectorAll('[data-type=\"Movie\"], [data-type=\"Series\"]');");
        sb.AppendLine("    debugLog(`Found ${cards.length} cards`);");
        sb.AppendLine("    cards.forEach(card => {");
        sb.AppendLine("      if (!card.hasAttribute('data-trailer-preview')) {");
        sb.AppendLine("        card.setAttribute('data-trailer-preview', 'true');");
        sb.AppendLine("        card.addEventListener('mouseenter', () => handleCardHover(card));");
        sb.AppendLine("        card.addEventListener('mouseleave', handleCardLeave);");
        sb.AppendLine("      }");
        sb.AppendLine("    });");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Initialize
        sb.AppendLine("  function init() {");
        sb.AppendLine("    debugLog('Initializing TrailerPreview v2...');");
        sb.AppendLine("    attachListeners();");
        sb.AppendLine();
        sb.AppendLine("    const observer = new MutationObserver(() => {");
        sb.AppendLine("      attachListeners();");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine("    observer.observe(document.body, {");
        sb.AppendLine("      childList: true,");
        sb.AppendLine("      subtree: true");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine("    debugLog('TrailerPreview initialized successfully');");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Wait for DOM
        sb.AppendLine("  if (document.readyState === 'loading') {");
        sb.AppendLine("    document.addEventListener('DOMContentLoaded', init);");
        sb.AppendLine("  } else {");
        sb.AppendLine("    init();");
        sb.AppendLine("  }");
        sb.AppendLine();

        sb.AppendLine("})();");

        return sb.ToString();
    }
}
