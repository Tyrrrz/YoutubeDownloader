using YoutubeExplode.Videos;

namespace YoutubeDownloader.Utils;

internal static class FileNameGenerator
{
    private static string NumberToken { get; } = "$num";

    private static string TitleToken { get; } = "$title";

    private static string AuthorToken { get; } = "$author";

    public static string DefaultTemplate { get; } = $"{TitleToken}";

    public static string GenerateFileName(string template, IVideo video, string extension, string? number = null) =>
        PathEx.EscapeFileName(
            template
                .Replace(NumberToken, !string.IsNullOrWhiteSpace(number) ? $"[{number}]" : "")
                .Replace(TitleToken, video.Title)
                .Replace(AuthorToken, video.Author.Title)
                .Trim() + '.' + extension
        );
}