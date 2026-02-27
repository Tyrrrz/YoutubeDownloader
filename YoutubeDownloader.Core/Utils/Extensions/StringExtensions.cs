namespace YoutubeDownloader.Core.Utils.Extensions;

public static class StringExtensions
{
    extension(string str)
    {
        public string? NullIfWhiteSpace() => !string.IsNullOrWhiteSpace(str) ? str : null;
    }
}
