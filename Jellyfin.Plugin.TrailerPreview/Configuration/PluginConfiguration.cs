using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TrailerPreview.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    // General Settings
    public bool EnableTrailerPreview { get; set; } = true;
    public bool EnableDebugLogging { get; set; } = false;

    // Timing Settings
    public int HoverDelayMs { get; set; } = 800;
    public int PreviewDurationMs { get; set; } = 30000;
    public int FadeInDurationMs { get; set; } = 300;
    public int FadeOutDurationMs { get; set; } = 200;

    // Display Settings
    public int PreviewWidthPercent { get; set; } = 40;
    public int PreviewHeightPercent { get; set; } = 60;
    public string PreviewPosition { get; set; } = "center";

    // Visual Settings
    public bool EnableBackgroundDim { get; set; } = true;
    public double BackgroundDimOpacity { get; set; } = 0.7;
    public double PreviewOpacity { get; set; } = 1.0;
    public bool ShowTitle { get; set; } = true;

    // Audio Settings
    public bool EnableAudio { get; set; } = false;
    public int AudioVolume { get; set; } = 30;

    // Platform Support
    public bool EnableOnWeb { get; set; } = true;
    public bool EnableOnDesktop { get; set; } = true;
    public bool EnableOnMobile { get; set; } = false; // Disabled by default to avoid mobile app bugs
    public bool EnableOnTV { get; set; } = true;
}
