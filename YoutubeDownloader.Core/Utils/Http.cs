using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace YoutubeDownloader.Core.Utils;

internal static class Http
{
    public static HttpClient Client { get; } = new()
    {
        DefaultRequestHeaders =
        {
            // Required by some of the services we're using
            UserAgent =
            {
                new ProductInfoHeaderValue(
                    "YoutubeDownloader",
                    Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
                )
            }
        }
    };
}