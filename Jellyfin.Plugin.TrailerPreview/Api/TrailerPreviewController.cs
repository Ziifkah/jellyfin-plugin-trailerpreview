using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Jellyfin.Plugin.TrailerPreview.Configuration;
using Jellyfin.Plugin.TrailerPreview.Helpers;
using Jellyfin.Plugin.TrailerPreview.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TrailerPreview.Api;

[ApiController]
[Route("TrailerPreview")]
public class TrailerPreviewController : ControllerBase
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<TrailerPreviewController> _logger;

    public TrailerPreviewController(
        ILibraryManager libraryManager,
        ILogger<TrailerPreviewController> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    [HttpGet("ClientScript")]
    [AllowAnonymous]
    [Produces("application/javascript")]
    public ActionResult<string> GetClientScript()
    {
        try
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null || !config.EnableTrailerPreview)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Trailer Preview plugin is not enabled",
                    StatusCode = 404
                });
            }

            var script = ScriptBuilder.BuildClientScript(config);
            return Content(script, "application/javascript");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating client script");
            return StatusCode(500, new ErrorResponse
            {
                Message = "Internal server error",
                StatusCode = 500
            });
        }
    }

    [HttpGet("TrailerInfo/{itemId}")]
    [AllowAnonymous]
    public ActionResult<TrailerInfo> GetTrailerInfo([FromRoute] Guid itemId)
    {
        try
        {
            var item = _libraryManager.GetItemById(itemId);
            if (item == null)
            {
                return NotFound(new ErrorResponse
                {
                    Message = $"Item with ID {itemId} not found",
                    StatusCode = 404
                });
            }

            var trailerInfo = new TrailerInfo
            {
                IsAvailable = false,
                TrailerType = "None"
            };

            // Try to get local trailers first
            if (item is Movie movie || item is Series series)
            {
                var baseItem = item as BaseItem;
                var localTrailers = baseItem?.GetExtras()
                    .Where(e => e.ExtraType == ExtraType.Trailer)
                    .ToList();

                if (localTrailers != null && localTrailers.Any())
                {
                    var trailer = localTrailers.First();
                    trailerInfo.TrailerId = trailer.Id.ToString();
                    trailerInfo.TrailerUrl = $"/Items/{trailer.Id}/Download?api_key={{API_KEY}}";
                    trailerInfo.TrailerType = "Local";
                    trailerInfo.Title = trailer.Name;
                    trailerInfo.RuntimeTicks = trailer.RunTimeTicks;
                    trailerInfo.IsLocal = true;
                    trailerInfo.IsAvailable = true;

                    if (Plugin.Instance?.Configuration.EnableDebugLogging == true)
                    {
                        _logger.LogInformation("Found local trailer for item {ItemId}: {TrailerUrl}", itemId, trailerInfo.TrailerUrl);
                    }

                    return trailerInfo;
                }

                // Try remote trailers
                var remoteTrailers = baseItem?.RemoteTrailers;
                if (remoteTrailers != null && remoteTrailers.Any())
                {
                    var remoteTrailer = remoteTrailers.First();
                    var url = remoteTrailer.Url;

                    if (url != null && url.Contains("youtube.com") || url?.Contains("youtu.be") == true)
                    {
                        var videoId = ExtractYouTubeVideoId(url);
                        if (!string.IsNullOrEmpty(videoId))
                        {
                            trailerInfo.TrailerId = videoId;
                            trailerInfo.TrailerUrl = $"https://www.youtube.com/embed/{videoId}?autoplay=1&mute=1&modestbranding=1&rel=0&enablejsapi=1";
                            trailerInfo.TrailerType = "Remote";
                            trailerInfo.Title = remoteTrailer.Name ?? item.Name;
                            trailerInfo.IsLocal = false;
                            trailerInfo.IsAvailable = true;

                            if (Plugin.Instance?.Configuration.EnableDebugLogging == true)
                            {
                                _logger.LogInformation("Found remote trailer for item {ItemId}: {TrailerUrl}", itemId, trailerInfo.TrailerUrl);
                            }

                            return trailerInfo;
                        }
                    }
                }
            }

            if (Plugin.Instance?.Configuration.EnableDebugLogging == true)
            {
                _logger.LogInformation("No trailer found for item {ItemId}", itemId);
            }

            return trailerInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trailer info for item {ItemId}", itemId);
            return StatusCode(500, new ErrorResponse
            {
                Message = "Internal server error",
                StatusCode = 500
            });
        }
    }

    [HttpGet("Status")]
    [AllowAnonymous]
    public ActionResult<TrailerPreviewStatus> GetStatus()
    {
        var config = Plugin.Instance?.Configuration;
        return new TrailerPreviewStatus
        {
            Enabled = config?.EnableTrailerPreview ?? false,
            HoverDelayMs = config?.HoverDelayMs ?? 800,
            Version = "1.0.0"
        };
    }

    private string? ExtractYouTubeVideoId(string url)
    {
        try
        {
            var uri = new Uri(url);

            if (uri.Host.Contains("youtube.com"))
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["v"];
            }

            if (uri.Host.Contains("youtu.be"))
            {
                return uri.AbsolutePath.TrimStart('/');
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract YouTube video ID from URL: {Url}", url);
        }

        return null;
    }
}
