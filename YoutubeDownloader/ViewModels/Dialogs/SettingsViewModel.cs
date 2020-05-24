using Tyrrrz.Extensions;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class SettingsViewModel : DialogScreen
    {
        private readonly SettingsService _settingsService;

        public bool IsAutoUpdateEnabled
        {
            get => _settingsService.IsAutoUpdateEnabled;
            set => _settingsService.IsAutoUpdateEnabled = value;
        }

        public int MaxConcurrentDownloads
        {
            get => _settingsService.MaxConcurrentDownloadCount;
            set => _settingsService.MaxConcurrentDownloadCount = value.Clamp(1, 10);
        }

        public string FileNameTemplate
        {
            get => _settingsService.FileNameTemplate;
            set => _settingsService.FileNameTemplate = value;
        }

        public bool ShouldInjectTags
        {
            get => _settingsService.ShouldInjectTags;
            set => _settingsService.ShouldInjectTags = value;
        }

        public bool ShouldSkipExistingFiles
        {
            get => _settingsService.ShouldSkipExistingFiles;
            set => _settingsService.ShouldSkipExistingFiles = value;
        }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }
    }
}