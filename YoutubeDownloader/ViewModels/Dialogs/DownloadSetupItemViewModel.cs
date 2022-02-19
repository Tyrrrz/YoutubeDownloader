using System.Collections.Generic;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSetupItemViewModel
{
    public bool IsSelected { get; set; } = true;

    public string? FilePath { get; set; }

    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption>? AvailableVideoOptions { get; set; }

    public VideoDownloadOption? VideoOption { get; set; }

    public IReadOnlyList<SubtitleDownloadOption>? AvailableSubtitleOptions { get; set; }

    public IReadOnlyList<SubtitleDownloadOption>? SubtitleOptions { get; set; }
}

public static class DownloadSetupItemViewModelExtensions
{
    public static DownloadSetupItemViewModel CreateDownloadSetupItemViewModel(
        this IViewModelFactory factory,
        IVideo video,
        IReadOnlyList<VideoDownloadOption> availableVideoOptions,
        IReadOnlyList<SubtitleDownloadOption> availableSubtitleOptions)
    {
        var viewModel = factory.CreateDownloadSetupItemViewModel();

        viewModel.Video = video;
        viewModel.AvailableVideoOptions = availableVideoOptions;
        viewModel.AvailableSubtitleOptions = availableSubtitleOptions;

        return viewModel;
    }
}