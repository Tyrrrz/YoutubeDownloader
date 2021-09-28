using System.Net.Http;
using System.Net.Http.Headers;

namespace YoutubeDownloader.Utils
{
    internal static class Http
    {
        public static HttpClient Client { get; } = new()
        {
            DefaultRequestHeaders =
            {
                UserAgent =
                {
                    new ProductInfoHeaderValue(App.Name, App.VersionString)
                }
            }
        };
    }
}