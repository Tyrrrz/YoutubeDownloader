using System;
using System.Text;
using System.Text.RegularExpressions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Utils
{
    internal static class FileNameGenerator
    {
        private static string NumberToken { get; } = "$num";

        private static string TitleToken { get; } = "$title";

        private static string AuthorToken { get; } = "$author";

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

            // Get rid of common rubbish in music video titles
            result = result.Replace("\"", "", StringComparison.OrdinalIgnoreCase);

            result = result.Replace("(official video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(official lyric video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(official music video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(official hd video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(official audio)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(orange version)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(official)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(lyric video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(lyrics)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(acoustic video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(acoustic)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(live)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(animated video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(video version)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(director's cut)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(original video)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(original video with subtitles)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(extended version)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(us version)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(closed captioned)", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("(edited)", "", StringComparison.OrdinalIgnoreCase);

            result = Regex.Replace(result, "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))", "");

            //result = result.Replace("[official video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[official lyric video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[official music video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[official hd video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[official audio]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[orange version]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[official]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[lyric video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[lyrics]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[acoustic video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[acoustic]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[live]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[animated video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[video version]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[director's cut]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[original video]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[original video with subtitles]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[extended version]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[us version]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[closed captioned]", "", StringComparison.OrdinalIgnoreCase);
            //result = result.Replace("[edited]", "", StringComparison.OrdinalIgnoreCase);

            result = result.Replace("official video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("official lyric video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("official music video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("official hd video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("official audio", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("orange version", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("lyric video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("acoustic video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("animated video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("video version", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("director's cut", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("original video", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("original video with subtitles", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("extended version", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("us version", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("closed captioned", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("lyrics", "", StringComparison.OrdinalIgnoreCase);

            result = result.Replace("__", "", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("   ", " ", StringComparison.OrdinalIgnoreCase);
            result = result.Replace("  ", " ", StringComparison.OrdinalIgnoreCase);


            result = RemoveEmoticon(result);

            result = result.Trim();

            result += $".{format}";

            return PathEx.EscapeFileName(result);
        }

        private static string RemoveEmoticon(string str)
        {
            foreach (var a in str)
            {
                byte[] bts = Encoding.UTF32.GetBytes(a.ToString());

                if (bts[0].ToString() == "253" && bts[1].ToString() == "255")
                {
                    str = str.Replace(a.ToString(), "");
                }

            }
            return str;
        }
    }
}