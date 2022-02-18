using System;
using System.Collections.Generic;
using System.Windows;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSingleSetupViewModel : DialogScreen<DownloadViewModel>
{
    private readonly SettingsService _settingsService;
    private readonly DialogManager _dialogManager;

    public string? Title { get; set; }

    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption> AvailableVideoOptions { get; set; } =
        Array.Empty<VideoDownloadOption>();

    public IReadOnlyList<SubtitleDownloadOption> AvailableSubtitleOptions { get; set; } =
        Array.Empty<SubtitleDownloadOption>();

    public VideoDownloadOption? SelectedVideoOption { get; set; }

    public IReadOnlyList<SubtitleDownloadOption>? SelectedSubtitleOptions { get; set; }

    public string? FilePath { get; set; }

    public DownloadSingleSetupViewModel(SettingsService settingsService, DialogManager dialogManager)
    {
        _settingsService = settingsService;
        _dialogManager = dialogManager;
    }

    public bool CanConfirm => SelectedVideoOption is not null;

    public void Confirm()
    {
        if (Video is null || SelectedVideoOption is null)
            return;

        var defaultFileName = FileNameGenerator.GenerateFileName(
            _settingsService.FileNameTemplate,
            Video,
            SelectedVideoOption.Container.Name
        );

        FilePath = _dialogManager.PromptSaveFilePath(
            $"{SelectedVideoOption.Container.Name.ToUpperInvariant()} file|*.{SelectedVideoOption.Container.Name}",
            defaultFileName
        );

        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        Close();
    }

    public void CopyTitle()
    {
        if (string.IsNullOrWhiteSpace(Title))
            return;

        Clipboard.SetText(Title);
    }
}

public static class DownloadSingleSetupViewModelExtensions
{
    public static DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(
        this IViewModelFactory factory,
        string title,
        IVideo video,
        IReadOnlyList<VideoDownloadOption> availableDownloadOptions,
        IReadOnlyList<SubtitleDownloadOption> availableSubtitleOptions)
    {
        var viewModel = factory.CreateDownloadSingleSetupViewModel();

        viewModel.Title = title;
        viewModel.Video = video;
        viewModel.AvailableVideoOptions = availableDownloadOptions;
        viewModel.AvailableSubtitleOptions = availableSubtitleOptions;

        return viewModel;
    }
}