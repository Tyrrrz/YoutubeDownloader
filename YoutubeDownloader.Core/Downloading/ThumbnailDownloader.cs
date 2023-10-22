using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Downloading
{
    public class ThumbnailDownloader
    {
        public async Task DownloadThumbnailAsync(
            string path,
            IVideo video,
            CancellationToken cancellationToken = default
        )
        {
            var tempPath = Path.ChangeExtension(path, "jpg");
            var thumbnailUrl =
                video.Thumbnails
                    .Where(
                        t =>
                            string.Equals(
                                t.TryGetImageFormat(),
                                "jpg",
                                StringComparison.OrdinalIgnoreCase
                            )
                    )
                    .OrderByDescending(t => t.Resolution.Area)
                    .Select(t => t.Url)
                    .FirstOrDefault() ?? $"https://i.ytimg.com/vi/{video.Id}/maxresdefault.jpg";
            await File.WriteAllBytesAsync(
                tempPath,
                await Http.Client.GetByteArrayAsync(thumbnailUrl, cancellationToken),
                cancellationToken
            );
        }
    }
}
