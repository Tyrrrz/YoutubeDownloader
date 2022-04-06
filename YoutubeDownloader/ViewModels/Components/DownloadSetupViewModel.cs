using System;
using System.Collections.Generic;
using System.Linq;
using Stylet;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public class DownloadSetupViewModel : PropertyChangedBase
{
    private readonly SettingsService _settingsService;

    public bool IsSelected { get; set; } = true;

    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    public VideoDownloadOption? SelectedDownloadOption { get; set; }

    public string? FilePath { get; set; }

    public DownloadSetupViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void RestoreDefaults()
    {
        // Last used format
        if (!string.IsNullOrWhiteSpace(_settingsService.LastFormat))
        {
            SelectedDownloadOption = AvailableDownloadOptions?
                .FirstOrDefault(o =>
                    string.Equals(o.Container.Name, _settingsService.LastFormat, StringComparison.OrdinalIgnoreCase)
                );
        }
    }

    public void OpenVideoPage() => ProcessEx.StartShellExecute(Video!.Url);
}

public static class DownloadSetupViewModelExtensions
{
    public static DownloadSetupViewModel CreateDownloadSetupViewModel(
        this IViewModelFactory factory,
        IVideo video,
        IReadOnlyList<VideoDownloadOption> availableDownloadOptions)
    {
        var viewModel = factory.CreateDownloadSetupViewModel();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = availableDownloadOptions;
        viewModel.SelectedDownloadOption = availableDownloadOptions.FirstOrDefault();

        return viewModel;
    }
}