using System.Linq;
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

        public bool IsDarkModeEnabled
        {
            get => _settingsService.IsDarkModeEnabled;
            set => _settingsService.IsDarkModeEnabled = value;
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

           public bool ShouldSkipEmptyFiles
           {
               get => _settingsService.ShouldSkipEmptyFiles;
               set => _settingsService.ShouldSkipEmptyFiles = value;
           }
           
           public bool SaveShortcutFile
           {
               get => _settingsService.SaveShortcutFile;
               set => _settingsService.SaveShortcutFile = value;
           }
           
           public string FileNameTemplateForSingleFile
           {
               get => _settingsService.FileNameTemplateForSingleFile;
               set => _settingsService.FileNameTemplateForSingleFile = value;
           }
           
           public string FileNameTemplateForMultipleFiles
           {
               get => _settingsService.FileNameTemplateForMultipleFiles;
               set => _settingsService.FileNameTemplateForMultipleFiles = value;
           }

        public string ExcludedContainerFormats
        {
            get => _settingsService.ExcludedContainerFormats is not null
                ? _settingsService.ExcludedContainerFormats.JoinToString(",")
                : "";

            set => _settingsService.ExcludedContainerFormats = value
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
        }

        public int MaxConcurrentDownloads
        {
            get => _settingsService.MaxConcurrentDownloadCount;
            set => _settingsService.MaxConcurrentDownloadCount = value.Clamp(1, 10);
        }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }
    }
}