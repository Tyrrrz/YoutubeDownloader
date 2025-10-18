using System;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.WebView.Windows.Core;
using Microsoft.Web.WebView2.Core;
using WebViewCore.Events;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupView : UserControl<AuthSetupViewModel>
{
    private const string HomePageUrl = "https://www.youtube.com";
    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={Uri.EscapeDataString(HomePageUrl)}";

    private CoreWebView2? _coreWebView2;

    public AuthSetupView() => InitializeComponent();

    private void NavigateToLoginPage() => WebBrowser.Url = new Uri(LoginPageUrl);

    private void LogOutButton_OnClick(object sender, RoutedEventArgs args)
    {
        DataContext.Cookies = null;
        NavigateToLoginPage();
    }

    private void WebBrowser_OnLoaded(object sender, RoutedEventArgs args) => NavigateToLoginPage();

    private void WebBrowser_OnWebViewCreated(object sender, WebViewCreatedEventArgs args)
    {
        if (!args.IsSucceed)
            return;

        var platformWebView = WebBrowser.PlatformWebView as WebView2Core;
        var coreWebView2 = platformWebView?.CoreWebView2;

        if (coreWebView2 is null)
            return;

        coreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        coreWebView2.Settings.AreDevToolsEnabled = false;
        coreWebView2.Settings.IsGeneralAutofillEnabled = false;
        coreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        coreWebView2.Settings.IsStatusBarEnabled = false;
        coreWebView2.Settings.IsSwipeNavigationEnabled = false;

        _coreWebView2 = coreWebView2;
    }

    private async void WebBrowser_OnNavigationStarting(
        object? sender,
        WebViewUrlLoadingEventArg args
    )
    {
        if (_coreWebView2 is null)
            return;

        // Reset existing browser cookies if the user is attempting to log in (again)
        if (string.Equals(args.Url?.AbsoluteUri, LoginPageUrl, StringComparison.OrdinalIgnoreCase))
            _coreWebView2.CookieManager.DeleteAllCookies();

        // Extract the cookies after being redirected to the home page (i.e. after logging in)
        if (
            args.Url?.AbsoluteUri.StartsWith(HomePageUrl, StringComparison.OrdinalIgnoreCase)
            == true
        )
        {
            var cookies = await _coreWebView2!.CookieManager.GetCookiesAsync(args.Url.AbsoluteUri);
            DataContext.Cookies = cookies.Select(c => c.ToSystemNetCookie()).ToArray();
        }
    }
}
