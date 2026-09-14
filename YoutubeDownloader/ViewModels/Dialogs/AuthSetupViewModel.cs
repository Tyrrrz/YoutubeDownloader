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
            .All(IsStillValid);

    /// <remarks>
    /// A cookie with no expiry is a session cookie, which lasts until the session ends
    /// rather than having already lapsed. Treating one as expired matters on Android:
    /// its CookieManager exposes only the "name=value" pairs from the cookie header, with
    /// no expiry at all, so every cookie arrives with <see cref="Cookie.Expires" /> left at
    /// its default. Requiring a date in the future can never be satisfied there, and the
    /// app reported itself as signed out immediately after a successful sign-in.
    /// </remarks>
    private static bool IsStillValid(Cookie cookie) =>
        !cookie.Expired
        && (cookie.Expires == default || cookie.Expires.ToUniversalTime() > DateTime.UtcNow);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventSubscription.Dispose();
        }

        base.Dispose(disposing);
    }
}
