using System;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupView
{
    private const string HomePageUrl = "https://www.youtube.com";
    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={WebUtility.UrlEncode(HomePageUrl)}";

    private AuthSetupViewModel ViewModel => (AuthSetupViewModel) DataContext;

    public AuthSetupView()
    {
        InitializeComponent();
    }

    private void NavigateToLoginPage() => WebBrowser.Source = new Uri(LoginPageUrl);

    private void LogoutHyperlink_OnClick(object sender, RoutedEventArgs args)
    {
        ViewModel.ResetCookies();
        NavigateToLoginPage();
    }

    private void WebBrowser_OnLoaded(object sender, RoutedEventArgs args) => NavigateToLoginPage();

    private void WebBrowser_OnCoreWebView2InitializationCompleted(
        object? sender,
        CoreWebView2InitializationCompletedEventArgs args)
    {
        if (!args.IsSuccess)
            return;

        WebBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        WebBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
        WebBrowser.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
        WebBrowser.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        WebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
        WebBrowser.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
    }

    private void WebBrowser_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs args)
    {
        // Reset existing browser cookies if the user is logging in (again)
        if (string.Equals(args.Uri, LoginPageUrl, StringComparison.OrdinalIgnoreCase))
            WebBrowser.CoreWebView2.CookieManager.DeleteAllCookies();
    }

    private async void WebBrowser_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        var url = WebBrowser.Source.AbsoluteUri.TrimEnd('/');

        // Navigated to the home page
        if (string.Equals(url, HomePageUrl, StringComparison.OrdinalIgnoreCase))
        {
            // Extract the cookies that the browser received after logging in
            var cookies = await WebBrowser.CoreWebView2.CookieManager.GetCookiesAsync(WebBrowser.Source.AbsoluteUri);
            ViewModel.SaveCookies(cookies.ToDictionary(i => i.Name, i => i.Value));
        }
    }
}