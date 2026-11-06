using System;

namespace YoutubeDownloader.ViewModels.Dialogs;

public enum VideoSortOption
{
    Default,
    TitleAscending,
    TitleDescending,
    DurationAscending,
    DurationDescending,
}

public static class VideoSortOptionExtensions
{
    extension(VideoSortOption option)
    {
        public string GetDisplayName() =>
            option switch
            {
                VideoSortOption.Default => "Default",
                VideoSortOption.TitleAscending => "Title (A → Z)",
                VideoSortOption.TitleDescending => "Title (Z → A)",
                VideoSortOption.DurationAscending => "Duration (shortest)",
                VideoSortOption.DurationDescending => "Duration (longest)",
                _ => throw new ArgumentOutOfRangeException(nameof(option)),
            };
    }
}
