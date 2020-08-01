using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Services;
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

        public string Title { get; set; }

        public IReadOnlyList<Video> AvailableVideos { get; set; }

        public IReadOnlyList<Video> SelectedVideos { get; set; }

        public IReadOnlyList<string> AvailableFormats { get; } = new[] {"mp4", "mp3", "ogg"};

        public string SelectedFormat { get; set; }

        public DownloadMultipleSetupViewModel(IViewModelFactory viewModelFactory, SettingsService settingsService,
            DialogManager dialogManager)
        {
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _dialogManager = dialogManager;
        }

        public void OnViewLoaded()
        {
            // Select last used format
            SelectedFormat = !string.IsNullOrWhiteSpace(_settingsService.LastFormat) && AvailableFormats.Contains(_settingsService.LastFormat)
                ? _settingsService.LastFormat
                : AvailableFormats.FirstOrDefault();
        }

        public bool CanConfirm => SelectedVideos != null && SelectedVideos.Any();

        public void Confirm()
        {
            // Prompt user for output directory path
            var dirPath = _dialogManager.PromptDirectoryPath();

            // If canceled - return
            if (string.IsNullOrWhiteSpace(dirPath))
                return;

            // Save last used format
            _settingsService.LastFormat = SelectedFormat;

            // Make sure selected videos are ordered in the same way as available videos
            var orderedSelectedVideos = AvailableVideos.Where(v => SelectedVideos.Contains(v)).ToArray();

            // Create download view models
            var downloads = new List<DownloadViewModel>();
            for (var i = 0; i < orderedSelectedVideos.Length; i++)
            {
                var video = orderedSelectedVideos[i];

                // Generate file path
                var number = (i + 1).ToString().PadLeft(orderedSelectedVideos.Length.ToString().Length, '0');
                var fileName = FileNameGenerator.GenerateFileName(_settingsService.FileNameTemplate, video, SelectedFormat, number);
                var filePath = Path.Combine(dirPath, fileName);

                // If file exists - either skip it or generate a unique file path, depending on user settings
                if (File.Exists(filePath))
                {
                    if (_settingsService.ShouldSkipExistingFiles)
                        continue;

                    filePath = PathEx.MakeUniqueFilePath(filePath);
                }

                // Create empty file to "lock in" the file path
                PathEx.CreateDirectoryForFile(filePath);
                PathEx.CreateEmptyFile(filePath);

                // Create download view model
                var download = _viewModelFactory.CreateDownloadViewModel(video, filePath, SelectedFormat);

                // Add to list
                downloads.Add(download);
            }

            // Close dialog
            Close(downloads);
        }

        public void CopyTitle() => Clipboard.SetText(Title);
    }
}