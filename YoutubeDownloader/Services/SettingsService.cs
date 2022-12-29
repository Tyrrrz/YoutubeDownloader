using Microsoft.Win32;
using Tyrrrz.Settings;
using YoutubeDownloader.Core.Downloading;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Services;

public partial class SettingsService : SettingsManager
{
    public bool IsAutoUpdateEnabled { get; set; } = true;
    public bool UsePreviewVersion { get; set; } = false;
    public bool IsDarkModeEnabled { get; set; } = IsDarkModeEnabledByDefault();

    public bool ShouldInjectTags { get; set; } = true;

    public bool ShouldSkipExistingFiles { get; set; }

    public string FileNameTemplate { get; set; } = "[$num]-[$title]-[$id]";

    public int ParallelLimit { get; set; } = 4;
    public bool DownloadMostViewedVideoOnly { get; set; } = true;

    public Container LastContainer { get; set; } = Container.Mp4;

    public VideoQualityPreference LastVideoQualityPreference { get; set; } = VideoQualityPreference.Highest;

    public SettingsService()
    {
        Configuration.StorageSpace = StorageSpace.Instance;
        Configuration.SubDirectoryPath = "";
        Configuration.FileName = "Settings.dat";
    }
}

public partial class SettingsService
{
    private static bool IsDarkModeEnabledByDefault()
    {
        try
        {
            return Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
                false
            )?.GetValue("AppsUseLightTheme") is 0;
        }
        catch
        {
            return false;
        }
    }
}