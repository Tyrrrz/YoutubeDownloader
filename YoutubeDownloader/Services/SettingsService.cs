using Microsoft.Win32;
using Tyrrrz.Settings;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader.Services;

public partial class SettingsService : SettingsManager
{
    public bool IsAutoUpdateEnabled { get; set; } = true;

    public bool IsDarkModeEnabled { get; set; } = IsDarkModeEnabledByDefault();

    public string FileNameTemplate { get; set; } = FileNameGenerator.DefaultTemplate;

    public int ParallelLimit { get; set; } = 2;

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
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", false
            )?.GetValue("AppsUseLightTheme") is 0;
        }
        catch
        {
            return false;
        }
    }
}