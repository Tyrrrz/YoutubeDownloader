using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PowerKit.Extensions;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class AuthSetupViewModel : DialogViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly IDisposable _eventSubscription;

    public AuthSetupViewModel(
        LocalizationManager localizationManager,
        SettingsService settingsService
    )
    {
        LocalizationManager = localizationManager;
        _settingsService = settingsService;

        _eventSubscription = _settingsService.WatchProperty(
            o => o.LastAuthCookies,
            _ =>
            {
                OnPropertyChanged(nameof(Cookies));
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        );
    }

    public LocalizationManager LocalizationManager { get; }

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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventSubscription.Dispose();
        }

        base.Dispose(disposing);
    }
}
