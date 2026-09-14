using System;
using System.Linq;
using System.Net;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using PowerKit.Extensions;
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

    public AuthSetupView() => InitializeComponent();

    /// <summary>
    /// Fills in the domain a cookie was read from, where the platform supplied none.
    /// </summary>
    /// <remarks>
    /// Android's CookieManager answers a URL with the cookie header for that URL and
    /// nothing else - no domain, no path, no expiry, because the platform API exposes none
    /// of them. IsApplicableFor compares against Domain, so every captured cookie was
    /// discarded: signing in appeared to work and then sent nothing, with the failure
    /// context on a download reading "Auth cookies: 0" right after a successful login.
    ///
    /// A cookie read from a page is by definition applicable to that page, so filling in
    /// the host it came from only states what the platform already implied.
    /// </remarks>
    private static Cookie AttributeToHomePage(Cookie cookie)
    {
        if (string.IsNullOrEmpty(cookie.Domain))
            cookie.Domain = HomePageUri.Host;

        return cookie;
    }

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
        // Reaching the home page is how the login flow ends: the sign-in pages live on
        // accounts.google.com and redirect here once they are done.
        var hasReachedHomePage =
            args.Request is { } url
            && string.Equals(url.Scheme, HomePageUri.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(url.Host, HomePageUri.Host, StringComparison.OrdinalIgnoreCase);

        if (WebBrowser.TryGetCookieManager() is { } cookieManager)
        {
            // Reset existing browser cookies if the user is attempting to log in (again)
            if (
                string.Equals(
                    args.Request?.AbsoluteUri,
                    LoginPageUri.AbsoluteUri,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                foreach (var cookie in await cookieManager.GetCookiesAsync())
                    cookieManager.DeleteCookie(cookie);
            }

            // Extract the cookies after being redirected to the home page (i.e., after logging in)
            if (hasReachedHomePage)
            {
                var cookies = await cookieManager.GetCookiesAsync();
                DataContext.Cookies = cookies
                    .Select(AttributeToHomePage)
                    .Where(c => c.IsApplicableFor(HomePageUri))
                    .ToArray();
            }
        }

        // Close once the flow is over rather than leaving a cramped copy of YouTube on
        // screen: the cookies are captured by then, and they are the only thing this dialog
        // exists to collect.
        //
        // Deliberately not conditional on IsAuthenticated. That asks whether the cookies
        // carry a future expiry date, which is a question about the cookies rather than
        // about the flow, and on Android the answer is always no - see the remarks there.
        // Hanging the close off it meant the dialog simply stayed open.
        if (hasReachedHomePage)
            DataContext.CloseCommand.ExecuteIfCan(true);
    }
}
