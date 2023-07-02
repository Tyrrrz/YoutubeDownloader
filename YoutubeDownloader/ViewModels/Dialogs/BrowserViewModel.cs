using System.Collections.Generic;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class BrowserViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;

    public BrowserViewModel(SettingsService settingsService) => _settingsService = settingsService;

    public Dictionary<string, string> Cookies
    {
        get => _settingsService.Cookies;
        set => _settingsService.Cookies = value;
    }
}