using System.Linq;
using Microsoft.Web.WebView2.Core;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class BrowserView
{
    private BrowserViewModel ViewModel => (BrowserViewModel) DataContext;
    
    public BrowserView()
    {
        InitializeComponent();
    }
    private async void WebBrowser_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        var cookies = await webBrowser.CoreWebView2.CookieManager.GetCookiesAsync("https://www.youtube.com");
        var isLogged = cookies?.Any(i => i.Name == "__Secure-3PSID") ?? false;
        
        if (isLogged)
        {
            ViewModel.Sapisid = cookies?.First(i => i.Name == "SAPISID")?.Value;
            ViewModel.Psid = cookies?.First(i => i.Name == "__Secure-3PSID")?.Value;
            webBrowser.CoreWebView2.CookieManager.DeleteAllCookies();
            webBrowser.Dispose();
            ViewModel.Close(true);
        }
    }
}