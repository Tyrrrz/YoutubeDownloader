using System.IO;
using YoutubeExplode.Common;

namespace YoutubeDownloader.Core.Utils.Extensions;

internal static class YoutubeExtensions
{
    public static string? TryGetImageFormat(this Thumbnail thumbnail) =>
        Url.TryExtractFileName(thumbnail.Url)?.Pipe(Path.GetExtension)?.Trim('.');
}