using System.IO;
using YoutubeExplode.Common;

namespace YoutubeDownloader.Core.Utils.Extensions;

internal static class YoutubeExtensions
{
    // TODO: can be removed with new version of YoutubeExplode
    public static string GetChannelUrl(this Author author) =>
        $"https://www.youtube.com/channel/{author.ChannelId}";

    public static string? TryGetImageFormat(this Thumbnail thumbnail) =>
        Url.TryExtractFileName(thumbnail.Url)?.Pipe(Path.GetExtension)?.Trim('.');
}