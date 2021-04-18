using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadSingleSetupViewModel : DialogScreen<DownloadViewModel>
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

        public string Title { get; set; } = default!;

        public IVideo Video { get; set; } = default!;

        public IReadOnlyList<VideoDownloadOption> AvailableVideoOptions { get; set; } =
            Array.Empty<VideoDownloadOption>();

        public IReadOnlyList<SubtitleDownloadOption> AvailableSubtitleOptions { get; set; } =
            Array.Empty<SubtitleDownloadOption>();

        public VideoDownloadOption? SelectedVideoOption { get; set; }

        public SubtitleDownloadOption? SelectedSubtitleOption { get; set; }

        public bool ShouldDownloadSubtitles { get; set; }

        public DownloadSingleSetupViewModel(
            IViewModelFactory viewModelFactory,
            SettingsService settingsService,
            DialogManager dialogManager)
        {
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _dialogManager = dialogManager;
        }

        public void OnViewLoaded()
        {
            // Select first video download option matching last used format or first non-audio-only download option
            SelectedVideoOption =
                AvailableVideoOptions.FirstOrDefault(o => o.Format == _settingsService.LastFormat) ??
                AvailableVideoOptions.OrderByDescending(o => !string.IsNullOrWhiteSpace(o.Label)).FirstOrDefault();

            // Select first subtitle download option matching last used language
            SelectedSubtitleOption =
                AvailableSubtitleOptions.FirstOrDefault(o =>
                    o.TrackInfo.Language.Code == _settingsService.LastSubtitleLanguageCode) ??
                AvailableSubtitleOptions.FirstOrDefault();
        }

        public bool CanConfirm => SelectedVideoOption is not null;

        public void Confirm()
        {
            var format = SelectedVideoOption!.Format;

            // Prompt for output file path
            var defaultFileName = FileNameGenerator.GenerateFileName(
                _settingsService.FileNameTemplate,
                Video,
                format
            );

            var filter = $"{format.ToUpperInvariant()} file|*.{format}";

            var filePath = _dialogManager.PromptSaveFilePath(filter, defaultFileName);
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            _settingsService.LastFormat = format;
            _settingsService.LastSubtitleLanguageCode = SelectedSubtitleOption?.TrackInfo.Language.Code;

            var download = _viewModelFactory.CreateDownloadViewModel(
                Video!,
                filePath,
                format,
                SelectedVideoOption,
                ShouldDownloadSubtitles ? SelectedSubtitleOption : null
            );

            // Create empty file to "lock in" the file path.
            // This is necessary as there may be other downloads with the same file name
            // which would otherwise overwrite the file.
            PathEx.CreateDirectoryForFile(filePath);
            PathEx.CreateEmptyFile(filePath);

            Close(download);
        }

        public void CopyTitle() => Clipboard.SetText(Title);
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
}