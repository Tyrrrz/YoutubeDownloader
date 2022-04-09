using System;
using System.Collections.Generic;
using System.IO;
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

    public void OnViewLoaded()
    {
        SelectedDownloadOption = AvailableDownloadOptions?.FirstOrDefault(o =>
            o.Container == _settingsService.LastContainer
        );
    }

    public void OpenVideoPage() => ProcessEx.StartShellExecute(Video!.Url);

    public void Confirm()
    {
        var container = SelectedDownloadOption!.Container;

        FilePath = _dialogManager.PromptSaveFilePath(
            $"{container.Name} file|*.{container.Name}",
            FileNameTemplate.Apply(
                _settingsService.FileNameTemplate,
                Video!,
                container
            )
        );

        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        // Download does not start immediately, so lock in the file path to avoid conflicts
        DirectoryEx.CreateDirectoryForFile(FilePath);
        File.WriteAllBytes(FilePath, Array.Empty<byte>());

        _settingsService.LastContainer = container;

        Close(true);
    }
}

public static class DownloadSingleSetupViewModelExtensions
{
    public static DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(
        this IViewModelFactory factory,
        IVideo video,
        IReadOnlyList<VideoDownloadOption> availableDownloadOptions)
    {
        var viewModel = factory.CreateDownloadSingleSetupViewModel();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = availableDownloadOptions;

        return viewModel;
    }
}