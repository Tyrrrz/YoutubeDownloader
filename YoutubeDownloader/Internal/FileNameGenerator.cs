using YoutubeExplode.Videos;

namespace YoutubeDownloader.Internal
{
    internal static class FileNameGenerator
    {
        private static string NumberToken { get; } = "$num";

        private static string TitleToken { get; } = "$title";

        private static string AuthorToken { get; } = "$author";

        private static string UploadDateToken { get; } = "$uploadDate";

        public static string DefaultTemplate { get; } = $"{TitleToken}";

        public static string GenerateFileName(string template, Video video, string format, string? number = null)
        {
            var result = template;

            result = result.Replace(NumberToken, !string.IsNullOrWhiteSpace(number) ? $"[{number}]" : "");
            result = result.Replace(TitleToken, video.Title);
            result = result.Replace(AuthorToken, video.Author);
            result = result.Replace(UploadDateToken, video.UploadDate.ToString("yyyy-MM-dd"));

            result = result.Trim();

            result += $".{format}";

            return FileEx.MakeSafeFileName(result);
        }
    }
}