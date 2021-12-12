using System;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Utils;

internal static class Youtube
{
    public static YoutubeClient Client { get; } = new(Http.Client);

    // TODO: can be removed with new version of YoutubeExplode.Converter
    public static bool IsAudioOnly(this Container container) =>
        string.Equals(container.Name, "mp3", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "m4a", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "wav", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "wma", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "ogg", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "aac", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(container.Name, "opus", StringComparison.OrdinalIgnoreCase);
}