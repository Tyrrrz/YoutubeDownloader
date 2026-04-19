using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using PowerKit.Extensions;

namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } =
        new()
        {
            DefaultRequestHeaders =
            {
                // Required by some of the services we're using
                UserAgent =
                {
                    new ProductInfoHeaderValue(
                        "YoutubeDownloader",
                        Assembly.GetExecutingAssembly().TryGetVersionString()
                    ),
                },
            },
        };
}
