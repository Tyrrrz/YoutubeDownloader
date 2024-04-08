using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class AuthSetupViewModel : DialogViewModelBase
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

        _settingsService.WatchProperty(
            o => o.LastAuthCookies,
            () =>
            {
                OnPropertyChanged(nameof(Cookies));
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        );
    }
}
