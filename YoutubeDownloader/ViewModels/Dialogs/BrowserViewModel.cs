using System;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class BrowserViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;

    public BrowserViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }


    public string? Sapisid
    {
        set => _settingsService.Sapisid = value;
    }
    
    public string? Psid
    {
        set => _settingsService.Psid = value;
    }
}