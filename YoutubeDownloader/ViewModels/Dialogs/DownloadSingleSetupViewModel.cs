using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSingleSetupViewModel : DialogScreen
{
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    public VideoDownloadOption? SelectedDownloadOption { get; set; }

    public string? FilePath { get; set; }

    public DownloadSingleSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
    {
        _dialogManager = dialogManager;
        _settingsService = settingsService;
    }

    public void OnViewFullyLoaded()
    {
        if (!string.IsNullOrWhiteSpace(_settingsService.LastFormat))
        {
            SelectedDownloadOption = AvailableDownloadOptions?
                .FirstOrDefault(o =>
                    string.Equals(o.Container.Name, _settingsService.LastFormat, StringComparison.OrdinalIgnoreCase)
                );
        }
    }

    public void OpenVideo()
    {
        var url = Video?.Url;
        if (!string.IsNullOrEmpty(url))
            ProcessEx.StartShellExecute(url);
    }

    public bool CanConfirm => SelectedDownloadOption is not null;

    public void Confirm()
    {
        if (Video is null || SelectedDownloadOption is null)
            return;

        var format = SelectedDownloadOption.Container.Name;

        FilePath = _dialogManager.PromptSaveFilePath(
            $"{format} file|*.{format}",
            FileNameTemplate.Apply(
                _settingsService.FileNameTemplate,
                Video,
                SelectedDownloadOption
            )
        );

        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        _settingsService.LastFormat = format;

        Close(true);
    }
}

public static class DownloadSingleSetupViewModelExtensions
{
    public static DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(
        this IViewModelFactory factory,
        IVideo video,
        IReadOnlyList<VideoDownloadOption> downloadOptions)
    {
        var viewModel = factory.CreateDownloadSetupViewModel();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = downloadOptions;
        viewModel.SelectedDownloadOption = downloadOptions.FirstOrDefault();

        return viewModel;
    }
}