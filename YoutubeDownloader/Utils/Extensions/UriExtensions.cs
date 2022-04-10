using System;

namespace YoutubeDownloader.Utils.Extensions;

internal static class UriExtensions
{
    public static string GetHostWithoutWww(this Uri uri)
    {
        var host = uri.Host;
        
        return host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
            ? host[4..]
            : host;
    }
}