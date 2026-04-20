using System;
using System.IO;
using PowerKit.Extensions;
using YoutubeExplode.Common;

namespace YoutubeDownloader.Core.Utils.Extensions;

public static class ThumbnailExtensions
{
    extension(Thumbnail thumbnail)
    {
        public string? TryGetImageFormat() =>
            new Uri(thumbnail.Url, UriKind.RelativeOrAbsolute)
                .TryGetFileName()
                ?.Pipe(Path.GetExtension)
                ?.Trim('.');
    }
}
