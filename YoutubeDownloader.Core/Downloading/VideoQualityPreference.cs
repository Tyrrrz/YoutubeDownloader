using System;

namespace YoutubeDownloader.Core.Downloading;

public enum VideoQualityPreference
{
    UpTo480p,
    UpTo720p,
    UpTo1080p,
    Highest
}

public static class VideoQualityPreferenceExtensions
{
    public static string GetDisplayName(this VideoQualityPreference preference) => preference switch
    {
        
        VideoQualityPreference.UpTo480p => "≤ 480p",
        VideoQualityPreference.UpTo720p => "≤ 720p",
        VideoQualityPreference.UpTo1080p => "≤ 1080p",
        VideoQualityPreference.Highest => "Cao nhất",
        _ => throw new ArgumentOutOfRangeException(nameof(preference))
    };
}