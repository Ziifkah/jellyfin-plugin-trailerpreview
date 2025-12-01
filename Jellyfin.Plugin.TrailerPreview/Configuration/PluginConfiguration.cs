using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TrailerPreview.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public bool EnableTrailerPreview { get; set; } = true;
    public bool EnableDebugLogging { get; set; } = false;

    // Timing settings
    public int HoverDelayMs { get; set; } = 800;
    public int PreviewDurationMs { get; set; } = 30000;
    public int FadeInDurationMs { get; set; } = 300;
    public int FadeOutDurationMs { get; set; } = 200;

    // Position settings
    public string PositionMode { get; set; } = "Center";
    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;

    // Size settings
    public string SizeMode { get; set; } = "Percentage";
    public int PreviewWidth { get; set; } = 800;
    public int PreviewHeight { get; set; } = 450;
    public int PreviewSizePercentage { get; set; } = 35;

    // Visual settings
    public double PreviewOpacity { get; set; } = 0.95;
    public int PreviewBorderRadius { get; set; } = 12;
    public bool EnableBackgroundBlur { get; set; } = true;
    public bool EnableBackgroundDim { get; set; } = true;
    public double BackgroundDimOpacity { get; set; } = 0.7;

    // Audio settings
    public bool EnableAudio { get; set; } = false;
    public int Volume { get; set; } = 30;
    public bool MuteByDefault { get; set; } = true;

    // Video settings
    public string VideoQuality { get; set; } = "hd720";
    public bool AutoPlayTrailer { get; set; } = true;
    public bool LoopTrailer { get; set; } = false;
    public bool PreloadTrailers { get; set; } = true;

    // UI settings
    public bool ShowProgressBar { get; set; } = true;
    public bool ShowCloseButton { get; set; } = true;
    public bool ShowMuteButton { get; set; } = true;
    public bool ShowTitle { get; set; } = true;

    // Platform-specific settings
    public bool EnableOnTV { get; set; } = true;
    public bool EnableOnMobile { get; set; } = true;
    public bool EnableOnDesktop { get; set; } = true;
    public bool EnableOnWeb { get; set; } = true;

    // Advanced settings
    public bool OnlyShowForItems { get; set; } = false;
    public string[] ItemTypes { get; set; } = new[] { "Movie", "Series" };
    public int MaxConcurrentPreviews { get; set; } = 1;
    public bool DisableOnLowBandwidth { get; set; } = true;
}
