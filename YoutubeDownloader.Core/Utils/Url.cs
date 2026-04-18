using System.Text.RegularExpressions;
using PowerKit.Extensions;

namespace YoutubeDownloader.Core.Utils;

public static class Url
{
    public static string? TryExtractFileName(string url) =>
        Regex.Match(url, @".+/([^?]*)").Groups[1].Value.NullIfWhiteSpace();
}
