using System.Collections.Generic;
using Tyrrrz.Settings;
using YoutubeDownloader.Models;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        public bool IsAutoUpdateEnabled { get; set; } = true;

        public bool IsDarkModeEnabled { get; set; }

        public bool ShouldInjectTags { get; set; } = true;

        public bool ShouldSkipExistingFiles { get; set; }

        public bool ShouldSkipEmptyFiles { get; set; } = false;

        public bool SaveShortcutFile { get; set; } = false;

        public string FileNameTemplateForSingleFile { get; set; } = FileNameGenerator.DefaultTemplate;

        public string FileNameTemplateForMultipleFiles { get; set; } = FileNameGenerator.DefaultTemplate;

        public IReadOnlyList<string>? ExcludedContainerFormats { get; set; }

        public int MaxConcurrentDownloadCount { get; set; } = 2;

        public string? LastFormat { get; set; }

        public string? LastSubtitleLanguageCode { get; set; }

        public VideoQualityPreference LastVideoQualityPreference { get; set; } = VideoQualityPreference.Maximum;

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}