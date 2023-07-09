using System.Collections.Generic;
using System.Linq;
using Stylet;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class AuthSetupViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;

    public bool IsAuthenticated => _settingsService.LastAuthCookies?.Any() == true;

    public AuthSetupViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        _settingsService.BindAndInvoke(
            o => o.LastAuthCookies,
            (_, _) => NotifyOfPropertyChange(() => IsAuthenticated)
        );
    }

    public void SaveCookies(IReadOnlyDictionary<string, string> cookies) =>
        _settingsService.LastAuthCookies = cookies.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public void ResetCookies() => _settingsService.LastAuthCookies = null;
}