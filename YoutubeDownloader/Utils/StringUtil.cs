using System.Text.RegularExpressions;

namespace YoutubeDownloader.Utils
{
    internal class StringUtil
    {
        public static bool IsChineseTitle(string title)
        {
            int chineseCharCount = Regex.Matches(title, @"[\u4e00-\u9fa5]").Count;
            double chineseCharPercentage = (double)chineseCharCount / title.Length;
            return chineseCharPercentage > 0.6;
        }
    }
}
