using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Downloading.Tagging;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

public class VideoDownloader
{
    private readonly YoutubeClient _youtube = new(Http.Client);
    private readonly MediaTagInjector _tagInjector = new();

    public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest);
    }

    private async Task DownloadAsync(
        string filePath,
        IVideo video,
        Container container,
        IReadOnlyList<IStreamInfo> streamInfos,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // If the target container supports subtitles, embed them in the video too
        var trackInfos = !container.IsAudioOnly
            ? (await _youtube.Videos.ClosedCaptions.GetManifestAsync(video.Id, cancellationToken)).Tracks
            : Array.Empty<ClosedCaptionTrackInfo>();

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        await _youtube.Videos.DownloadAsync(
            streamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath)
                .SetContainer(container)
                .SetPreset(ConversionPreset.Medium)
                .Build(),
            progress?.ToDoubleBased(),
            cancellationToken
        );

        try
        {
            await _tagInjector.InjectTagsAsync(filePath, video, cancellationToken);
        }
        catch
        {
            // Not critical, ignore
        }
    }

    public async Task DownloadAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption downloadOption,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default) =>
        await DownloadAsync(
            filePath,
            video,
            downloadOption.Container,
            downloadOption.StreamInfos,
            progress,
            cancellationToken
        );
}