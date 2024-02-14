using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using WebViewCore.Events;
using Xilium.CefGlue;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Events;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views.Components;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupView : UserControlBase
{
    private const string HomePageUrl = "https://www.youtube.com";
    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={WebUtility.UrlEncode(HomePageUrl)}";

    private AvaloniaCefBrowser browser;

    private AuthSetupViewModel ViewModel => (AuthSetupViewModel)DataContext!;

    public AuthSetupView()
    {
        InitializeComponent();
        browser = new AvaloniaCefBrowser();
        browser.Address = LoginPageUrl;
        browser.LoadStart += OnCefGlueBrowserLoadStart;
        browser.TitleChanged += OnCefGlueBrowserTitleChanged;
        browser.AddressChanged += OnCefGlueBrowserAddressChanged;
        WebPanel.Children.Add(browser);
    }

    private async void OnCefGlueBrowserAddressChanged(object sender, string address)
    {
        var url = address.TrimEnd('/');

        // Navigated to the home page (presumably after a successful login)
        if (string.Equals(url, HomePageUrl, StringComparison.OrdinalIgnoreCase))
        {
            // Extract the cookies that the browser received after logging in
            var context = CefRequestContext.GetGlobalContext();
            var cookieMangager = CefCookieManager.GetGlobal(null);
            var cookieVisitor = new YCookieVisitor();
            var couldAcccessCookies = cookieMangager.VisitUrlCookies(url, false, cookieVisitor);
            if (couldAcccessCookies)
            {
                await cookieVisitor.Completion;
                var cookies = cookieVisitor.CefCookies;
                Dispatcher.UIThread.Post(() =>
                {
                    ViewModel.Cookies = cookies.Select(c => c.ToSystemNetCookie()).ToArray();
                    var cl = ViewModel.Cookies.Select(c => new { c.Expires, c.Name }).ToArray();
                    string s = JsonSerializer.Serialize(
                        ViewModel.Cookies,
                        new JsonSerializerOptions() { WriteIndented = true }
                    );
                });
            }
        }
    }

    private void OnCefGlueBrowserTitleChanged(object sender, string title) { }

    private void OnCefGlueBrowserLoadStart(object sender, LoadStartEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TopLevel topLevel = TopLevel.GetTopLevel(this)!;
            if (topLevel != null)
            {
                topLevel!.Width++;
                topLevel!.Width--;
            }
        });
        //NavigateToLoginPage();
    }

    private void NavigateToLoginPage()
    {
        browser.Address = LoginPageUrl;

        //WebBrowser.Url = new Uri(LoginPageUrl);
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
        {
            return;
        }
    }

    // TODO
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

public class YCookieVisitor : CefCookieVisitor
{
    private readonly TaskCompletionSource _taskCompletionSource = new();

    public Task Completion => _taskCompletionSource.Task;

    public List<CefCookie> CefCookies { get; } = new();

    protected override bool Visit(CefCookie cookie, int count, int total, out bool delete)
    {
        CefCookies.Add(cookie);
        delete = false;

        if (count == total - 1)
        {
            _taskCompletionSource.SetResult();
        }

        return true;
    }
}

public static class CefExtensions
{
    private static readonly DateTime _cefTimeBegin = new DateTime(1601, 1, 1);

    public static Cookie ToSystemNetCookie(this CefCookie cefCookie)
    {
        return new Cookie(cefCookie.Name, cefCookie.Value, cefCookie.Path, cefCookie.Domain)
        {
            Expires = cefCookie.Expires.HasValue
                ? _cefTimeBegin.AddMicroseconds(cefCookie.Expires.Value.Ticks)
                : default,
            HttpOnly = cefCookie.HttpOnly,
            Secure = cefCookie.Secure
        };
    }
}
