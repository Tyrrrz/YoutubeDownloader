using System;
using System.IO;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Utils.Extensions;

internal static class YoutubeExtensions
{
    // TODO: can be removed with new version of YoutubeExplode.Converter
    public static bool IsAudioOnly(this Container container) =>
        string.Equals(container.Name, "mp3", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "m4a", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "wav", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "wma", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "ogg", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "aac", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "opus", StringComparison.OrdinalIgnoreCase);

    public static string? TryGetImageFormat(this Thumbnail thumbnail) =>
        Url.TryExtractFileName(thumbnail.Url)?.Pipe(Path.GetExtension)?.Trim('.');
}