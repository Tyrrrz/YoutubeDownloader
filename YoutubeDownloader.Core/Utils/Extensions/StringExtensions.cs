namespace YoutubeDownloader.Core.Utils.Extensions;

public static class StringExtensions
{
    extension(string str)
    {
        public string? NullIfEmptyOrWhiteSpace() => !string.IsNullOrEmpty(str.Trim()) ? str : null;
    }
}
