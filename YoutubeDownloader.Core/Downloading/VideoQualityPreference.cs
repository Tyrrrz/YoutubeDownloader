using System;

namespace YoutubeDownloader.Core.Downloading;

public enum VideoQualityPreference
{
    Lowest,
    Low,
    Medium,
    High,
    Highest
}

public static class VideoQualityPreferenceExtensions
{
    public static string GetDisplayName(this VideoQualityPreference preference) => preference switch
    {
        VideoQualityPreference.Lowest => "Lowest",
        VideoQualityPreference.Low => "<= 480p",
        VideoQualityPreference.Medium => "<= 720p",
        VideoQualityPreference.High => "<= 1080p",
        VideoQualityPreference.Highest => "Highest",
        _ => throw new ArgumentOutOfRangeException(nameof(preference))
    };
}