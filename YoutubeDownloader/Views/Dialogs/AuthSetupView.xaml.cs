using System;
using System.Net;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PropertyChanged;
using WebViewCore.Events;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

[DoNotNotify]
public partial class AuthSetupView : UserControl
{
    private const string HomePageUrl = "https://www.youtube.com";
    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={WebUtility.UrlEncode(HomePageUrl)}";

    private AuthSetupViewModel ViewModel => (AuthSetupViewModel)DataContext!;

    public AuthSetupView()
    {
        InitializeComponent();
    }

    private void NavigateToLoginPage() => WebBrowser.Url = new Uri(LoginPageUrl);

    private void LogoutHyperlink_OnClick(object sender, RoutedEventArgs args)
    {
        ViewModel.Cookies = null;
        NavigateToLoginPage();
    }

    private void WebBrowser_OnLoaded(object sender, RoutedEventArgs args) => NavigateToLoginPage();

    private void WebBrowser_OnWebViewCreated(object sender, WebViewCreatedEventArgs args)
    {
        if (!args.IsSucceed)
        {
            return;
        }
    }

    //private void WebBrowser_OnCoreWebView2InitializationCompleted(
    //    object? sender,
    //    CoreWebView2InitializationCompletedEventArgs args)
    //{
    //    if (!args.IsSuccess)
    //        return;

    //    WebBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
    //    WebBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
    //    WebBrowser.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
    //    WebBrowser.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
    //    WebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
    //    WebBrowser.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
    //}

    //private void WebBrowser_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs args)
    //{
    //    // Reset existing browser cookies if the user is attempting to log in (again)
    //    if (string.Equals(args.Uri, LoginPageUrl, StringComparison.OrdinalIgnoreCase))
    //        WebBrowser.CoreWebView2.CookieManager.DeleteAllCookies();
    //}

    //private async void WebBrowser_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs args)
    //{
    //    var url = WebBrowser.Source.AbsoluteUri.TrimEnd('/');

    //    // Navigated to the home page (presumably after a successful login)
    //    if (string.Equals(url, HomePageUrl, StringComparison.OrdinalIgnoreCase))
    //    {
    //        // Extract the cookies that the browser received after logging in
    //        var cookies = await WebBrowser.CoreWebView2.CookieManager.GetCookiesAsync(url);
    //        ViewModel.Cookies = cookies.Select(c => c.ToSystemNetCookie()).ToArray();
    //    }
    //}

    //private async void WebBrowser_OnUnloaded(object sender, RoutedEventArgs args)
    //{
    //    // This will most likely not work because WebView2 would still be running at this point,
    //    // and there doesn't seem to be any way to shut it down using the .NET API.
    //    if (WebBrowser.CoreWebView2?.Profile is not null)
    //        await WebBrowser.CoreWebView2.Profile.ClearBrowsingDataAsync();
    //}
}