using System.Linq;
using Microsoft.Web.WebView2.Core;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class BrowserView
{
    public BrowserView()
    {
        InitializeComponent();
    }

    private BrowserViewModel ViewModel => (BrowserViewModel) DataContext;

    private async void WebBrowser_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        var cookies = await WebBrowser.CoreWebView2.CookieManager.GetCookiesAsync("https://www.youtube.com");
        var isOnYoutube = WebBrowser.Source.Host == "www.youtube.com";

        if (cookies.Any() && isOnYoutube)
        {
            var cookiesDic = cookies!.ToDictionary(i => i.Name, i => i.Value);
            ViewModel.Cookies = cookiesDic;
            WebBrowser.CoreWebView2.CookieManager.DeleteAllCookies();
            WebBrowser.Dispose();
            ViewModel.Close();
        }
    }
}