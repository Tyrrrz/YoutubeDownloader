using System.IO;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.ClosedCaptions;
namespace YoutubeDownloader.Core.Downloading;

public class VideoDownloader
{
    private readonly YoutubeClient _youtube = new(Http.Client);

    public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest);
    }

    public async Task<VideoDownloadOption> GetBestDownloadOptionAsync(
        VideoId videoId,
        VideoDownloadPreference preference,
        CancellationToken cancellationToken = default)
    {
        var options = await GetDownloadOptionsAsync(videoId, cancellationToken);

        return
            preference.TryGetBestOption(options) ??
            throw new InvalidOperationException("No suitable download option found.");
    }

    public async Task DownloadAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption downloadOption,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // If the target container supports subtitles, embed them in the video too
        var trackInfos = !downloadOption.Container.IsAudioOnly
            ? (await _youtube.Videos.ClosedCaptions.GetManifestAsync(video.Id, cancellationToken)).Tracks
            : Array.Empty<ClosedCaptionTrackInfo>();

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        await _youtube.Videos.DownloadAsync(
            downloadOption.StreamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath)
                .SetContainer(downloadOption.Container)
                .SetPreset(ConversionPreset.Medium)
                .Build(),
            progress?.ToDoubleBased(),
            cancellationToken
        );
    }
}