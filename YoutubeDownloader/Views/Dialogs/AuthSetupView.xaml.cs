using System;
using System.Linq;
using System.Net;
using Avalonia.Interactivity;
using Microsoft.Web.WebView2.Core;
using WebViewCore.Events;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views.Framework;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupView : ViewModelAwareUserControl<AuthSetupViewModel>
{
    private const string HomePageUrl = "https://www.youtube.com";
    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={WebUtility.UrlEncode(HomePageUrl)}";

    private CoreWebView2? _coreWebView2;

    public AuthSetupView()
    {
        InitializeComponent();
    }

    private void NavigateToLoginPage()
    {
        WebBrowser.Url = new Uri(LoginPageUrl);
    }

    private void LogoutHyperlink_OnClick(object sender, RoutedEventArgs args)
    {
        ViewModel.Cookies = null;
        NavigateToLoginPage();
    }

    private void WebBrowser_OnLoaded(object sender, RoutedEventArgs args) => NavigateToLoginPage();

    private void WebBrowser_OnWebViewCreated(object sender, WebViewCreatedEventArgs args)
    {
        if (!args.IsSucceed)
            return;

        var platformWebView = (
            (Avalonia.WebView.Windows.Core.WebView2Core)WebBrowser.PlatformWebView!
        );
        _coreWebView2 = platformWebView!.CoreWebView2!;

        _coreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        _coreWebView2.Settings.AreDevToolsEnabled = false;
        _coreWebView2.Settings.IsGeneralAutofillEnabled = false;
        _coreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        _coreWebView2.Settings.IsStatusBarEnabled = false;
        _coreWebView2.Settings.IsSwipeNavigationEnabled = false;
    }

    private async void WebBrowser_OnNavigationStarting(
        object? sender,
        WebViewUrlLoadingEventArg args
    )
    {
        // Reset existing browser cookies if the user is attempting to log in (again)
        if (string.Equals(args.Url?.AbsoluteUri, LoginPageUrl, StringComparison.OrdinalIgnoreCase))
            _coreWebView2!.CookieManager.DeleteAllCookies();

        if (args.Url!.AbsoluteUri.StartsWith(HomePageUrl, StringComparison.OrdinalIgnoreCase))
        {
            // Extract the cookies that the browser received after logging in
            var cookies = await _coreWebView2!.CookieManager.GetCookiesAsync(
                args.Url!.AbsoluteUri!
            );
            ViewModel.Cookies = cookies.Select(c => c.ToSystemNetCookie()).ToArray();
        }
    }
}
