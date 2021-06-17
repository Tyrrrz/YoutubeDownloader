using System.Collections.Generic;
using Tyrrrz.Settings;
using YoutubeDownloader.Models;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        //shazam1 "ce9b4f1b07msh30e430fb450d74cp1287c6jsn95656bec26ac"
        //shazam2 "61528e733dmshb9547e5ed1d9c22p152034jsnc276dbfff781"
        //vagalume "660a4395f992ff67786584e238f501aa"

        public bool IsAutoUpdateEnabled { get; set; } = true;

        public bool IsDarkModeEnabled { get; set; }

        public bool ShouldInjectTags { get; set; } = true;

        public bool AutoRenameFile { get; set; } = true;

        public bool ShouldSkipExistingFiles { get; set; }

        public string FileNameTemplate { get; set; } = FileNameGenerator.DefaultTemplate;

        public IReadOnlyList<string> ExcludedContainerFormats { get; set; }

        public int MaxConcurrentDownloadCount { get; set; } = 2;

        public string LastFormat { get; set; }

        public string LastSubtitleLanguageCode { get; set; }

        public string FastAPIShazamKeys { get; set; }

        public string VagalumeAPIKeys { get; set; } = "660a4395f992ff67786584e238f501aa";

        public VideoQualityPreference LastVideoQualityPreference { get; set; } = VideoQualityPreference.Maximum;

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}