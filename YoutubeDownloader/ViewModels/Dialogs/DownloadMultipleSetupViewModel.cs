using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tyrrrz.Extensions;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadMultipleSetupViewModel : DialogScreen<IReadOnlyList<DownloadViewModel>>
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

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

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            // Select last used format
            SelectedFormat = AvailableFormats.Contains(_settingsService.LastFormat)
                ? _settingsService.LastFormat
                : AvailableFormats.FirstOrDefault();
        }

        public bool CanConfirm => !SelectedVideos.IsNullOrEmpty();

        public void Confirm()
        {
            // Prompt user for output directory path
            var dirPath = _dialogManager.PromptDirectoryPath();

            // If canceled - return
            if (dirPath.IsNullOrWhiteSpace())
                return;

            // Save last used format
            _settingsService.LastFormat = SelectedFormat;

            // Create download view models
            var downloads = new List<DownloadViewModel>();
            foreach (var video in SelectedVideos)
            {
                // Generate file path
                var fileName = $"{video.GetFileNameSafeTitle()}.{SelectedFormat}";
                var filePath = Path.Combine(dirPath, fileName);

                // Ensure file paths are unique because users will not be able to confirm overwrites
                filePath = FileEx.MakeUniqueFilePath(filePath);

                // Create empty file to "lock in" the file path
                FileEx.CreateEmptyFile(filePath);

                // Create download view model
                var download = _viewModelFactory.CreateDownloadViewModel(video, filePath, SelectedFormat);

                // Add to list
                downloads.Add(download);
            }

            // Close dialog
            Close(downloads);
        }
    }
}