using Tyrrrz.Settings;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader.Services;

public class SettingsService : SettingsManager
{
    public bool IsAutoUpdateEnabled { get; set; } = true;

    public bool IsDarkModeEnabled { get; set; }

    public bool ShouldSkipExistingFiles { get; set; }

    public string FileNameTemplate { get; set; } = FileNameGenerator.DefaultTemplate;

    public int ParallelLimit { get; set; } = 2;

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