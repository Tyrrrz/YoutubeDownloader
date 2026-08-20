using System;
using System.Net.Http;

namespace YoutubeExplode.Utils;

internal static class Http
{
    private static readonly Lazy<HttpClient> HttpClientLazy = new(() => new HttpClient());

    public static HttpClient Client => HttpClientLazy.Value;
}
