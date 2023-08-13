using Avalonia.Media;

namespace YoutubeDownloader.Utils;

internal static class MediaColor
{
    public static Color FromHex(string hex) => Color.Parse(hex);
}