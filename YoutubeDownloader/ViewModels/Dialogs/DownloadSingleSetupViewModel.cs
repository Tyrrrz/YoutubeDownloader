using System.Collections.Generic;
using System.Linq;
using Tyrrrz.Extensions;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadSingleSetupViewModel : DialogScreen<DownloadViewModel>
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

        public Video Video { get; set; }

        public IReadOnlyList<DownloadOption> AvailableDownloadOptions { get; set; }

        public DownloadOption SelectedDownloadOption { get; set; }

        public DownloadSingleSetupViewModel(IViewModelFactory viewModelFactory, SettingsService settingsService,
            DialogManager dialogManager)
        {
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _dialogManager = dialogManager;
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            // Select first download option matching last used format or first non-audio-only download option
            SelectedDownloadOption =
                AvailableDownloadOptions.FirstOrDefault(o => o.Format == _settingsService.LastFormat) ??
                AvailableDownloadOptions.OrderByDescending(o => !o.Label.IsNullOrWhiteSpace()).FirstOrDefault();
        }

        public bool CanConfirm => Video != null;

        public void Confirm()
        {
            var format = SelectedDownloadOption.Format;

            // Prompt user for output file path
            var filter = $"{format.ToUpperInvariant()} file|*.{format}";
            var defaultFileName = $"{Video.GetFileNameSafeTitle()}.{format}";
            var filePath = _dialogManager.PromptSaveFilePath(filter, defaultFileName);

            // If canceled - return
            if (filePath.IsNullOrWhiteSpace())
                return;

            // Save last used format
            _settingsService.LastFormat = format;

            // Create download view model
            var download = _viewModelFactory.CreateDownloadViewModel(Video, filePath, format, SelectedDownloadOption);

            // Create empty file to "lock in" the file path
            FileEx.CreateEmptyFile(filePath);

            // Close dialog
            Close(download);
        }
    }
}