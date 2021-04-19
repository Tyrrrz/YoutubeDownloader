using System.Windows.Media;

namespace YoutubeDownloader.Utils
{
    internal static class MediaColor
    {
        public static Color FromHex(string hex) => (Color) ColorConverter.ConvertFromString(hex);
    }
}