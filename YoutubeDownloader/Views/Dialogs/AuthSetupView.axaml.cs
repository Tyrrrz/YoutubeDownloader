using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupView : UserControl<AuthSetupViewModel>
{
    private static readonly Uri HomePageUri = new("https://www.youtube.com");
    private static readonly Uri LoginPageUri = new(
        $"https://accounts.google.com/ServiceLogin?continue={Uri.EscapeDataString(HomePageUri.AbsoluteUri)}"
    );
    private bool _isRetryingLoginNavigation;

    public AuthSetupView() => InitializeComponent();

    private void NavigateToLoginPage() => WebBrowser.Source = LoginPageUri;

    private void LogOutButton_OnClick(object sender, RoutedEventArgs args)
    {
        DataContext.Cookies = null;
        NavigateToLoginPage();
    }

    private void WebBrowser_OnLoaded(object sender, RoutedEventArgs args) => NavigateToLoginPage();

    private void WebBrowser_OnEnvironmentRequested(
        object? sender,
        WebViewEnvironmentRequestedEventArgs args
    )
    {
        args.EnableDevTools = false;

        switch (args)
        {
            case WindowsWebView2EnvironmentRequestedEventArgs windowsArgs:
                windowsArgs.IsInPrivateModeEnabled = true;
                break;
            case AppleWKWebViewEnvironmentRequestedEventArgs appleArgs:
                appleArgs.NonPersistentDataStore = true;
                break;
            case GtkWebViewEnvironmentRequestedEventArgs gtkArgs:
                gtkArgs.EphemeralDataManager = true;
                break;
        }
    }

    private async void WebBrowser_OnNavigationStarted(
        object? sender,
        WebViewNavigationStartingEventArgs args
    )
    {
        if (WebBrowser.TryGetCookieManager() is not { } cookieManager)
            return;

        // Reset existing browser cookies if the user is attempting to log in (again)
        if (
            string.Equals(
                args.Request?.AbsoluteUri,
                LoginPageUri.AbsoluteUri,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            if (_isRetryingLoginNavigation)
            {
                _isRetryingLoginNavigation = false;
            }
            else
            {
                args.Cancel = true;

                foreach (var cookie in await cookieManager.GetCookiesAsync())
                    cookieManager.DeleteCookie(cookie);

                _isRetryingLoginNavigation = true;
                NavigateToLoginPage();

                return;
            }
        }

        // Extract the cookies after being redirected to the home page (i.e., after logging in)
        if (
            args.Request is { } url
            && string.Equals(url.Scheme, HomePageUri.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(url.Host, HomePageUri.Host, StringComparison.OrdinalIgnoreCase)
        )
        {
            var cookies = await cookieManager.GetCookiesAsync();
            DataContext.Cookies = cookies.Where(c => c.IsApplicableFor(HomePageUri)).ToArray();
        }
    }
}
