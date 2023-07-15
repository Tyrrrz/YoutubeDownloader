using System.Net.Http;

namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } = new();
}