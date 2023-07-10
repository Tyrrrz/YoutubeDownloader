using System.Collections.Generic;
using System.Linq;
using Stylet;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class AuthSetupViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;

    public IReadOnlyDictionary<string, string>? Cookies
    {
        get => _settingsService.LastAuthCookies;
        set => _settingsService.LastAuthCookies = value;
    }

    public bool IsAuthenticated => Cookies?.Any() == true;

    public AuthSetupViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        _settingsService.BindAndInvoke(
            o => o.LastAuthCookies,
            (_, _) => NotifyOfPropertyChange(() => Cookies)
        );

        this.BindAndInvoke(
            o => o.Cookies,
            (_, _) => NotifyOfPropertyChange(() => IsAuthenticated)
        );
    }
}