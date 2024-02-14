namespace YoutubeDownloader.Core.Utils.Extensions;

public static class StringExtensions
{
    public static string? NullIfEmptyOrWhiteSpace(this string str) =>
        !string.IsNullOrEmpty(str.Trim()) ? str : null;
}
