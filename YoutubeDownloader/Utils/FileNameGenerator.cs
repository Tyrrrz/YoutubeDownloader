using YoutubeExplode.Videos;

namespace YoutubeDownloader.Utils
{
    internal static class FileNameGenerator
    {
        private static string NumberToken { get; } = "$num";

        private static string TitleToken { get; } = "$title";

        private static string AuthorToken { get; } = "$author";

        private static string URLToken { get; } = "$url";
        public static string DefaultTemplate { get; } = $"{TitleToken}";

        public static string GenerateFileName(
            string template,
            IVideo video,
            string format,
            string? number = null)
        {
            var result = template;

            result = result.Replace(NumberToken, !string.IsNullOrWhiteSpace(number) ? $"[{number}]" : "");
            result = result.Replace(TitleToken, video.Title);
            result = result.Replace(AuthorToken, video.Author.Title);
            result = result.Replace(URLToken, video.Url.Substring(video.Url.IndexOf("v=") + 2));

            result = result.Trim();

            result += $".{format}";

            return PathEx.EscapeFileName(result);
        }
    }
}