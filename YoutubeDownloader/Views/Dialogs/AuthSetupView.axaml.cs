using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using WebViewCore.Events;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class AuthSetupView : UserControl<AuthSetupViewModel>
{
    private const string HomePageUrl = "https://www.youtube.com";
    private static readonly string LoginPageUrl =
        $"https://accounts.google.com/ServiceLogin?continue={Uri.EscapeDataString(HomePageUrl)}";

    private object? _platformWebView;

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

        _platformWebView = WebBrowser.PlatformWebView;

        if (_platformWebView is null)
            return;

        ConfigureWebViewSettings();
    }

    private void ConfigureWebViewSettings()
    {
        if (_platformWebView is null)
            return;

        try
        {
            var webViewType = _platformWebView.GetType();
            var typeName = webViewType.FullName ?? webViewType.Name;

            if (typeName.Contains("WebView2"))
            {
                ConfigureWebView2Settings();
            }
            else if (typeName.Contains("WebKit"))
            {
                ConfigureWebKitSettings();
            }
            else if (typeName.Contains("Android"))
            {
                ConfigureAndroidWebViewSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to configure WebView settings: {ex.Message}"
            );
        }
    }

    private void ConfigureWebView2Settings()
    {
        try
        {
            var coreWebView2Property = _platformWebView?.GetType().GetProperty("CoreWebView2");
            var coreWebView2 = coreWebView2Property?.GetValue(_platformWebView);

            if (coreWebView2 is not null)
            {
                var settingsProperty = coreWebView2.GetType().GetProperty("Settings");
                var settings = settingsProperty?.GetValue(coreWebView2);

                if (settings is not null)
                {
                    SetProperty(settings, "AreDefaultContextMenusEnabled", false);
                    SetProperty(settings, "AreDevToolsEnabled", false);
                    SetProperty(settings, "IsGeneralAutofillEnabled", false);
                    SetProperty(settings, "IsPasswordAutosaveEnabled", false);
                    SetProperty(settings, "IsStatusBarEnabled", false);
                    SetProperty(settings, "IsSwipeNavigationEnabled", false);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to configure WebView2: {ex.Message}");
        }
    }

    private void ConfigureWebKitSettings()
    {
        try
        {
            var configurationType = _platformWebView?.GetType().GetProperty("Configuration");
            var configuration = configurationType?.GetValue(_platformWebView);

            if (configuration is not null)
            {
                SetProperty(configuration, "AllowsInlineMediaPlayback", false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to configure WebKit: {ex.Message}");
        }
    }

    private void ConfigureAndroidWebViewSettings()
    {
        try
        {
            var settingsProperty = _platformWebView?.GetType().GetProperty("Settings");
            var settings = settingsProperty?.GetValue(_platformWebView);

            if (settings is not null)
            {
                SetProperty(settings, "JavaScriptEnabled", true);
                SetProperty(settings, "DomStorageEnabled", true);
                SetProperty(settings, "LoadWithOverviewMode", true);
                SetProperty(settings, "UseWideViewPort", true);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to configure Android WebView: {ex.Message}"
            );
        }
    }

    private static void SetProperty(object target, string propertyName, object value)
    {
        try
        {
            var property = target.GetType().GetProperty(propertyName);
            if (property?.CanWrite == true)
            {
                property.SetValue(target, value);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Failed to set property {propertyName}: {ex.Message}"
            );
        }
    }

    private async void WebBrowser_OnNavigationStarting(
        object? sender,
        WebViewUrlLoadingEventArg args
    )
    {
        if (_platformWebView is null)
            return;

        try
        {
            // Reset existing browser cookies if the user is attempting to log in (again)
            if (
                string.Equals(
                    args.Url?.AbsoluteUri,
                    LoginPageUrl,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                await ClearCookiesAsync();
            }

            // Extract the cookies after being redirected to the home page (i.e. after logging in)
            if (
                args.Url?.AbsoluteUri.StartsWith(HomePageUrl, StringComparison.OrdinalIgnoreCase)
                == true
            )
            {
                var cookies = await GetCookiesAsync(args.Url.AbsoluteUri);
                DataContext.Cookies = cookies;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    private async Task ClearCookiesAsync()
    {
        if (_platformWebView is null)
            return;

        try
        {
            var webViewType = _platformWebView.GetType();
            var typeName = webViewType.FullName ?? webViewType.Name;

            if (typeName.Contains("WebView2"))
            {
                ClearWebView2CookiesAsync();
            }
            else if (typeName.Contains("Android"))
            {
                ClearAndroidCookiesAsync();
            }
            else
            {
                await ClearGenericCookiesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear cookies: {ex.Message}");
        }
    }

    private void ClearWebView2CookiesAsync()
    {
        try
        {
            var coreWebView2Property = _platformWebView?.GetType().GetProperty("CoreWebView2");
            var coreWebView2 = coreWebView2Property?.GetValue(_platformWebView);

            if (coreWebView2 is not null)
            {
                var cookieManagerProperty = coreWebView2.GetType().GetProperty("CookieManager");
                var cookieManager = cookieManagerProperty?.GetValue(coreWebView2);

                if (cookieManager is not null)
                {
                    var deleteAllMethod = cookieManager.GetType().GetMethod("DeleteAllCookies");
                    deleteAllMethod?.Invoke(cookieManager, null);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear WebView2 cookies: {ex.Message}");
        }
    }

    private static void ClearAndroidCookiesAsync()
    {
        try
        {
            var cookieManagerType = Type.GetType("Android.Webkit.CookieManager, Mono.Android");
            if (cookieManagerType is not null)
            {
                var instanceMethod = cookieManagerType.GetMethod("GetInstance");
                var cookieManager = instanceMethod?.Invoke(null, null);

                if (cookieManager is not null)
                {
                    var removeAllMethod = cookieManager.GetType().GetMethod("RemoveAllCookies");
                    removeAllMethod?.Invoke(cookieManager, null);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear Android cookies: {ex.Message}");
        }
    }

    private async Task ClearGenericCookiesAsync()
    {
        try
        {
            var executeScriptMethod =
                _platformWebView?.GetType().GetMethod("ExecuteScriptAsync")
                ?? _platformWebView?.GetType().GetMethod("InvokeScriptAsync");

            if (executeScriptMethod is not null)
            {
                var script =
                    "document.cookie.split(';').forEach(function(c) { document.cookie = c.replace(/^ +/, '').replace(/=.*/, '=;expires=' + new Date().toUTCString() + ';path=/'); });";
                await (Task)(
                    executeScriptMethod.Invoke(_platformWebView, [script]) ?? Task.CompletedTask
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear generic cookies: {ex.Message}");
        }
    }

    private async Task<Cookie[]> GetCookiesAsync(string url)
    {
        if (_platformWebView is null)
            return [];

        try
        {
            var webViewType = _platformWebView.GetType();
            var typeName = webViewType.FullName ?? webViewType.Name;

            if (typeName.Contains("WebView2"))
            {
                return await GetWebView2CookiesAsync(url);
            }
            else if (typeName.Contains("Android"))
            {
                return GetAndroidCookiesAsync(url);
            }
            else
            {
                return await GetGenericCookiesAsync(url);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get cookies: {ex.Message}");
            return [];
        }
    }

    private async Task<Cookie[]> GetWebView2CookiesAsync(string url)
    {
        try
        {
            var coreWebView2Property = _platformWebView?.GetType().GetProperty("CoreWebView2");
            var coreWebView2 = coreWebView2Property?.GetValue(_platformWebView);

            if (coreWebView2 is not null)
            {
                var cookieManagerProperty = coreWebView2.GetType().GetProperty("CookieManager");
                var cookieManager = cookieManagerProperty?.GetValue(coreWebView2);

                if (cookieManager is not null)
                {
                    var getCookiesMethod = cookieManager.GetType().GetMethod("GetCookiesAsync");
                    var cookiesTask = getCookiesMethod?.Invoke(cookieManager, [url]);

                    if (cookiesTask is Task task)
                    {
                        await task;
                        var resultProperty = task.GetType().GetProperty("Result");
                        var cookies = resultProperty?.GetValue(task);

                        if (cookies is System.Collections.IEnumerable enumerable)
                        {
                            return
                            [
                                .. enumerable
                                    .Cast<object>()
                                    .Select(ConvertToSystemNetCookie)
                                    .Where(c => c is not null)
                                    .Cast<Cookie>(),
                            ];
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get WebView2 cookies: {ex.Message}");
        }

        return [];
    }

    private static Cookie[] GetAndroidCookiesAsync(string url)
    {
        try
        {
            // Android cookie extraction using reflection
            var cookieManagerType = Type.GetType("Android.Webkit.CookieManager, Mono.Android");
            if (cookieManagerType is not null)
            {
                var instanceMethod = cookieManagerType.GetMethod("GetInstance");
                var cookieManager = instanceMethod?.Invoke(null, null);

                if (cookieManager is not null)
                {
                    var getCookieMethod = cookieManager
                        .GetType()
                        .GetMethod("GetCookie", [typeof(string)]);
                    var cookieString = getCookieMethod?.Invoke(cookieManager, [url]) as string;

                    if (!string.IsNullOrEmpty(cookieString))
                    {
                        return ParseCookieString(cookieString, url);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get Android cookies: {ex.Message}");
        }

        return [];
    }

    private async Task<Cookie[]> GetGenericCookiesAsync(string url)
    {
        try
        {
            var executeScriptMethod =
                _platformWebView?.GetType().GetMethod("ExecuteScriptAsync")
                ?? _platformWebView?.GetType().GetMethod("InvokeScriptAsync");

            if (executeScriptMethod is not null)
            {
                var script = "document.cookie";
                var result = await (Task<string>)(
                    executeScriptMethod.Invoke(_platformWebView, [script]) ?? Task.FromResult("")
                );

                if (!string.IsNullOrEmpty(result))
                {
                    return ParseCookieString(result, url);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get generic cookies: {ex.Message}");
        }

        return [];
    }

    private static Cookie[] ParseCookieString(string cookieString, string url)
    {
        try
        {
            var uri = new Uri(url);
            var cookies = new List<Cookie>();

            var cookiePairs = cookieString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in cookiePairs)
            {
                var parts = pair.Trim().Split('=', 2);
                if (parts.Length == 2)
                {
                    var cookie = new Cookie(parts[0].Trim(), parts[1].Trim(), "/", uri.Host);
                    cookies.Add(cookie);
                }
            }

            return [.. cookies];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to parse cookie string: {ex.Message}");
            return [];
        }
    }

    private static Cookie? ConvertToSystemNetCookie(object webViewCookie)
    {
        try
        {
            var cookieType = webViewCookie.GetType();
            var name = cookieType.GetProperty("Name")?.GetValue(webViewCookie) as string;
            var value = cookieType.GetProperty("Value")?.GetValue(webViewCookie) as string;
            var domain = cookieType.GetProperty("Domain")?.GetValue(webViewCookie) as string;
            var path = cookieType.GetProperty("Path")?.GetValue(webViewCookie) as string;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                return new Cookie(name, value, path ?? "/", domain ?? "");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to convert cookie: {ex.Message}");
        }

        return null;
    }
}
