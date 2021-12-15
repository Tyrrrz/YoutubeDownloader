namespace YoutubeDownloader.Core.Utils.Extensions;

internal static class StringExtensions
{
    public static string? NullIfEmptyOrWhiteSpace(this string str) =>
        !string.IsNullOrEmpty(str.Trim())
            ? str
            : null;
}