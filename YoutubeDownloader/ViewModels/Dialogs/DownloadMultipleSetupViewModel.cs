using System;
using System.Collections.Generic;
using System.IO;
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
    public class DownloadMultipleSetupViewModel : DialogScreen<IReadOnlyList<DownloadViewModel>>
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

        public string Title { get; set; } = default!;

        public IReadOnlyList<IVideo> AvailableVideos { get; set; } = Array.Empty<IVideo>();

        public IReadOnlyList<IVideo> SelectedVideos { get; set; } = Array.Empty<IVideo>();

        public IReadOnlyList<string> AvailableFormats { get; set; } = new[] { "mp4", "mp3", "ogg" };

        public IReadOnlyList<VideoQualityPreference> AvailableQualityPreferences { get; } =
            Enum.GetValues(typeof(VideoQualityPreference)).Cast<VideoQualityPreference>().ToArray();

        public string? SelectedFormat { get; set; }

        public VideoQualityPreference SelectedVideoQualityPreference { get; set; } = VideoQualityPreference.Maximum;

        public bool IsAudioOnlyFormatSelected =>
            string.Equals(SelectedFormat, "mp3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(SelectedFormat, "ogg", StringComparison.OrdinalIgnoreCase);

        public DownloadMultipleSetupViewModel(
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
            if (_settingsService.ExcludedContainerFormats is not null)
                AvailableFormats = new[] {"mp4", "mp3", "ogg"}
                    .Where(f => !_settingsService.ExcludedContainerFormats?.Contains(f, StringComparer.OrdinalIgnoreCase) == true)
                    .ToArray();

            SelectedFormat =
                !string.IsNullOrWhiteSpace(_settingsService.LastFormat) &&
                AvailableFormats.Contains(_settingsService.LastFormat, StringComparer.OrdinalIgnoreCase)
                    ? _settingsService.LastFormat
                    : AvailableFormats.FirstOrDefault();

            SelectedVideoQualityPreference = _settingsService.LastVideoQualityPreference;
        }

        public bool CanConfirm => SelectedVideos.Any() && !string.IsNullOrWhiteSpace(SelectedFormat);

        public void Confirm()
        {
            // Prompt for output directory path
            var dirPath = _dialogManager.PromptDirectoryPath();
            if (string.IsNullOrWhiteSpace(dirPath))
                return;

            _settingsService.LastFormat = SelectedFormat;
            _settingsService.LastVideoQualityPreference = SelectedVideoQualityPreference;

            // Make sure selected videos are ordered in the same way as available videos
            var orderedSelectedVideos = AvailableVideos.Where(v => SelectedVideos.Contains(v)).ToArray();

            var downloads = new List<DownloadViewModel>();
            for (var i = 0; i < orderedSelectedVideos.Length; i++)
            {
                var video = orderedSelectedVideos[i];

                var fileName = FileNameGenerator.GenerateFileName(
                    _settingsService.FileNameTemplate,
                    video,
                    SelectedFormat!,
                    (i + 1).ToString().PadLeft(orderedSelectedVideos.Length.ToString().Length, '0')
                );

                var filePath = Path.Combine(dirPath, fileName);

                // If file exists or is no empty - either skip it or generate a unique file path, depending on user settings
                var fileInfo = new FileInfo(filePath);

                if (fileInfo.Exists && fileInfo.Length > 0)
                {
                    if (_settingsService.ShouldSkipExistingFiles)
                        continue;

                    filePath = PathEx.MakeUniqueFilePath(filePath);
                }

                // Create empty file to "lock in" the file path.
                // This is necessary as there may be other downloads with the same file name
                // which would otherwise overwrite the file.
                PathEx.CreateDirectoryForFile(filePath);
                PathEx.CreateEmptyFile(filePath);

                var download = _viewModelFactory.CreateDownloadViewModel(
                    video,
                    filePath,
                    SelectedFormat!,
                    SelectedVideoQualityPreference
                );

                downloads.Add(download);
            }

            Close(downloads);
        }

        public void CopyTitle() => Clipboard.SetText(Title);
    }

    public static class DownloadMultipleSetupViewModelExtensions
    {
        public static DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel(
            this IViewModelFactory factory,
            string title,
            IReadOnlyList<IVideo> availableVideos)
        {
            var viewModel = factory.CreateDownloadMultipleSetupViewModel();

            viewModel.Title = title;
            viewModel.AvailableVideos = availableVideos;

            return viewModel;
        }
    }
}
