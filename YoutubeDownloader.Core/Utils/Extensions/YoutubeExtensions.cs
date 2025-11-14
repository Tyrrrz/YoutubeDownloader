using System.IO;
using YoutubeExplode.Common;

namespace YoutubeDownloader.Core.Utils.Extensions;

public static class YoutubeExtensions
{
    extension(Thumbnail thumbnail)
    {
        public string? TryGetImageFormat() =>
            Url.TryExtractFileName(thumbnail.Url)?.Pipe(Path.GetExtension)?.Trim('.');
    }
}
