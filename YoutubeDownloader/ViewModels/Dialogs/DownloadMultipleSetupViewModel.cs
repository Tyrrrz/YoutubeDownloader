using System.Collections.Generic;
using System.Linq;
using Tyrrrz.Extensions;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadMultipleSetupViewModel : DialogScreen
    {
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

        public IReadOnlyList<Video> AvailableVideos { get; set; }

        public IReadOnlyList<Video> SelectedVideos { get; set; }

        public IReadOnlyList<string> AvailableFormats { get; } = new[] {"mp4", "mp3"};

        public string SelectedFormat { get; set; }

        public string DirPath { get; set; }

        public DownloadMultipleSetupViewModel(SettingsService settingsService, DialogManager dialogManager)
        {
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

        public bool CanConfirm => SelectedVideos != null && SelectedVideos.Count > 0;

        public void Confirm()
        {
            // Prompt user for output directory path
            DirPath = _dialogManager.PromptDirectoryPath();

            // If canceled - return
            if (DirPath.IsBlank())
                return;

            // Save last used format
            _settingsService.LastFormat = SelectedFormat;

            // Close dialog
            Close(true);
        }
    }
}