using System;
using System.IO;
using PowerKit.Extensions;
using YoutubeExplode.Common;

namespace YoutubeDownloader.Core.Utils.Extensions;

public static class ThumbnailExtensions
{
    extension(Thumbnail thumbnail)
    {
        public string? TryGetImageFormat()
        {
            if (!Uri.TryCreate(thumbnail.Url, UriKind.RelativeOrAbsolute, out var uri))
                return null;

            return uri.TryGetFileName()?.Pipe(Path.GetExtension)?.Trim('.');
        }
    }
}
