using System;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core;

public record DownloadOption(VideoDownloadOption Video, SubtitleDownloadOption? Subtitle)
{
    public async Task DownloadAsync(
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {

    }
}