using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ReactiveUI;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class AuthSetupViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;

    public IReadOnlyList<Cookie>? Cookies
    {
        get => _settingsService.LastAuthCookies;
        set => _settingsService.LastAuthCookies = value;
    }

    public bool IsAuthenticated =>
        Cookies?.Any() == true
        &&
        // None of the cookies should be expired
        Cookies.All(c => !c.Expired && (c.Expires == default || c.Expires > DateTimeOffset.Now));

    public AuthSetupViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        _settingsService
            .WhenAnyValue(o => o.LastAuthCookies)
            .Subscribe(_ => OnPropertyChanged(nameof(Cookies)));

        this.WhenAnyValue(o => o.Cookies)
            .Subscribe(_ => OnPropertyChanged(nameof(IsAuthenticated)));
    }
}
