using YoutubeExplode.Videos;

namespace YoutubeDownloader.Utils
{
    internal static class FileNameGenerator
    {
        private static string NumberToken { get; } = "$num"; // First video = 1, Second video = 2, Third video = 3, ...

        private static string InvertedNumberToken { get; } = "$numInverted"; // First video = N, Second video = N - 1, Third video = N - 2, ...  (where N = number of videos)

        private static string TitleToken { get; } = "$title";

        private static string AuthorToken { get; } = "$author";

        public static string DefaultTemplate { get; } = $"{TitleToken}";

        public static string GenerateFileName(
            string template,
            IVideo video,
            string format,
            string? number = null,
            string? inverted_number = null)
        {
            var result = template;

            result = result.Replace(InvertedNumberToken, !string.IsNullOrWhiteSpace(number) ? $"{inverted_number}" : "");
            result = result.Replace(NumberToken, !string.IsNullOrWhiteSpace(number) ? $"{number}" : "");// I removed the square brackets from the original code because they can be specified in the format string ir brackets are desired.
            result = result.Replace(TitleToken, video.Title);
            result = result.Replace(AuthorToken, video.Author.Title);

            result = result.Trim();

            result += $".{format}";

            return PathEx.EscapeFileName(result);
        }
    }
}