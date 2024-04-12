using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class AuthSetupViewModel : DialogViewModelBase
{
    private readonly SettingsService _settingsService;

    private readonly DisposableCollector _eventRoot = new();

    public IReadOnlyList<Cookie>? Cookies
    {
        get => _settingsService.LastAuthCookies;
        set => _settingsService.LastAuthCookies = value;
    }

    public bool IsAuthenticated =>
        Cookies?.Any() == true
        &&
        // None of the '__SECURE' cookies should be expired
        Cookies
            .Where(c => c.Name.StartsWith("__SECURE", StringComparison.OrdinalIgnoreCase))
            .All(c => !c.Expired && c.Expires.ToUniversalTime() > DateTime.UtcNow);

    public AuthSetupViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.LastAuthCookies,
                () =>
                {
                    OnPropertyChanged(nameof(Cookies));
                    OnPropertyChanged(nameof(IsAuthenticated));
                }
            )
        );
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventRoot.Dispose();
        }

        base.Dispose(disposing);
    }
}
