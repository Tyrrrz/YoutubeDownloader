using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Downloading.Tagging;

internal class MediaTagInjector
{
    private readonly MusicBrainzClient _musicBrainz = new();

    private void InjectMiscMetadata(MediaFile mediaFile, IVideo video)
    {
        var description = (video as Video)?.Description;
        if (!string.IsNullOrWhiteSpace(description))
            mediaFile.SetDescription(description);

        mediaFile.SetComment(
            "Downloaded from YouTube using YoutubeDownloader" + Environment.NewLine +
            $"Video: {video.Title}" + Environment.NewLine +
            $"Video URL: {video.Url}" + Environment.NewLine +
            $"Channel: {video.Author.Title}" + Environment.NewLine +
            $"Channel URL: {video.Author.ChannelUrl}"
        );
    } 
    
    private async Task InjectMusicMetadataAsync(
        MediaFile mediaFile,
        IVideo video,
        CancellationToken cancellationToken = default)
    {
        // We need duration for confidence rating and all videos are expected to have it anyway
        // since we're not working with live streams.
        if (video.Duration is not { } videoDuration)
            return;

        var recordings = await _musicBrainz.SearchRecordingsAsync(video.Title, cancellationToken);

        // MusicBrainz does not provide any kind of absolute confidence rating, so we're
        // going to choose the best fit by comparing video and recording durations.
        var recording = recordings
            .Select(r => new
            {
                Recording = r,
                // Rating range: (-inf, 1] where 1 is a perfect match
                ConfidenceRating = 1 - (videoDuration - r.Duration).Duration() / r.Duration
            })
            // Accepted discrepancy: ~8 seconds on a 3 minute song
            .Where(x => x.ConfidenceRating >= 0.95)
            .OrderByDescending(x => x.ConfidenceRating)
            .Select(r => r.Recording)
            .FirstOrDefault();

        if (recording is null)
            return;

        mediaFile.SetArtist(recording.Artist);
        mediaFile.SetTitle(recording.Title);

        if (!string.IsNullOrWhiteSpace(recording.ArtistSort))
            mediaFile.SetArtistSort(recording.ArtistSort);

        if (!string.IsNullOrWhiteSpace(recording.Album))
            mediaFile.SetAlbum(recording.Album);
    }

    private async Task InjectThumbnailAsync(
        MediaFile mediaFile,
        IVideo video,
        CancellationToken cancellationToken = default)
    {
        var thumbnailUrl =
            video.Thumbnails
                .OrderByDescending(t =>
                    string.Equals(
                        t.TryGetImageFormat(),
                        "jpg",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ThenByDescending(t => t.Resolution.Area)
                .Select(t => t.Url)
                .FirstOrDefault() ??
            $"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg";

        mediaFile.SetThumbnail(
            await Http.Client.GetByteArrayAsync(thumbnailUrl, cancellationToken)
        );
    }
    
    public async Task InjectTagsAsync(
        string filePath,
        IVideo video,
        CancellationToken cancellationToken = default)
    {
        using var mediaFile = MediaFile.Create(filePath);

        InjectMiscMetadata(mediaFile, video);
        await InjectMusicMetadataAsync(mediaFile, video, cancellationToken);
        await InjectThumbnailAsync(mediaFile, video, cancellationToken);
    }
}