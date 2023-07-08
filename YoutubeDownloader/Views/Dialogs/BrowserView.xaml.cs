using System.Linq;
using JsonExtensions;
using Microsoft.Web.WebView2.Core;
using YoutubeDownloader.Core.Utils;
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
        var isOnYoutube = WebBrowser.Source.AbsoluteUri == "https://www.youtube.com/";

        if (cookies.Any() && isOnYoutube)
        {
            var cookiesDic = cookies!.ToDictionary(i => i.Name, i => i.Value);
            ViewModel.Cookies = cookiesDic;
            
            var result = await Http.Client.GetStringAsync("https://www.youtube.com/getDatasyncIdsEndpoint");
            var datasyncIds = string.Join("\n",result.Split("\n").Skip(1));
            var dataSyncIdJson = Json.TryParse(datasyncIds);
            var dataSyncId = dataSyncIdJson?.GetProperty("responseContext").GetProperty("mainAppWebResponseContext").GetProperty("datasyncId").GetString()?.Split("||");
            var isRequired = dataSyncId?.Length >= 2 && !string.IsNullOrWhiteSpace(dataSyncId?[0]) && !string.IsNullOrWhiteSpace(dataSyncId[1]);
            var id = dataSyncId?[0];
            if (isRequired && id is not null) ViewModel.PageId = id;


            await WebBrowser.CoreWebView2.Profile.ClearBrowsingDataAsync();
            WebBrowser.Dispose();
            ViewModel.Close();
        }
    }
}