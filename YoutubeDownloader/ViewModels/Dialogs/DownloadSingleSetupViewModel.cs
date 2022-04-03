using System.Collections.Generic;
using System.Linq;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSingleSetupViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;
    private readonly DialogManager _dialogManager;

    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    public VideoDownloadOption? SelectedDownloadOption { get; set; }
    
    public string? FilePath { get; set; }

    public DownloadSingleSetupViewModel(SettingsService settingsService, DialogManager dialogManager)
    {
        _settingsService = settingsService;
        _dialogManager = dialogManager;
    }

    public void NavigateToVideo()
    {
        var url = Video?.Url;
        if (!string.IsNullOrEmpty(url))
            ProcessEx.StartShellExecute(url);
    }

    public bool CanConfirm => SelectedDownloadOption is not null;

    public void Confirm()
    {
        var format = SelectedDownloadOption!.Container.Name;

        // Prompt for output file path
        var defaultFileName = FileNameGenerator.GenerateFileName(
            _settingsService.FileNameTemplate,
            Video!,
            format
        );

        var filter = $"{format.ToUpperInvariant()} file|*.{format}";

        FilePath = _dialogManager.PromptSaveFilePath(filter, defaultFileName);
        if (string.IsNullOrWhiteSpace(FilePath))
            return;

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
        var viewModel = factory.CreateDownloadSetupViewModel();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = availableDownloadOptions;
        viewModel.SelectedDownloadOption = availableDownloadOptions.FirstOrDefault();

        return viewModel;
    }
}