namespace Jellyfin.Plugin.TrailerPreview.Models;

public class TrailerInfo
{
    public string? TrailerId { get; set; }
    public string? TrailerUrl { get; set; }
    public string TrailerType { get; set; } = "None";
    public string? Title { get; set; }
    public long? RuntimeTicks { get; set; }
    public bool IsLocal { get; set; }
    public bool IsAvailable { get; set; }
}

public class TrailerPreviewStatus
{
    public bool Enabled { get; set; }
    public int HoverDelayMs { get; set; }
    public string Version { get; set; } = "1.0.0";
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public int StatusCode { get; set; }
}
