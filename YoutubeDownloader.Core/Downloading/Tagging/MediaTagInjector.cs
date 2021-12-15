using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos;
using MediaFile = TagLib.File;

namespace YoutubeDownloader.Core.Downloading.Tagging;

internal class MediaTagInjector
{
    private readonly MusicBrainzClient _musicBrainz = new();

    private async Task InjectThumbnailAsync(
        MediaFile mediaFile,
        IVideo video,
        CancellationToken cancellationToken = default)
    {
        var thumbnailUrl = video.Thumbnails
            .OrderByDescending(t => string.Equals(t.TryGetImageFormat(), "jpg", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(t => t.Resolution.Area)
            .Select(t => t.Url)
            .FirstOrDefault();

        if (thumbnailUrl is null)
            return;

        var thumbnailData = await Http.Client.GetByteArrayAsync(thumbnailUrl, cancellationToken);
        mediaFile.SetThumbnail(thumbnailData);
    }

    private async Task InjectRecordingMetadataAsync(
        MediaFile mediaFile,
        IVideo video,
        CancellationToken cancellationToken = default)
    {
        // We need duration for confidence rating and all videos are expected to have it anyway
        // since we're not working with live streams.
        if (video.Duration is not { } videoDuration)
            return;

        // Use MusicBrainz to automatically resolve tags based on video title
        var recordings = await _musicBrainz.FindRecordingsAsync(video.Title, cancellationToken);

        // MusicBrainz does not provide any kind of confidence rating, so we're
        // going to choose the best fit by comparing video and recording durations.
        var recording = recordings
            .Select(r => new
            {
                Recording = r,
                ConfidenceRating = 1 - (videoDuration - r.Duration).Duration() / r.Duration
            })
            // Accepted discrepancy: ~25 seconds on a 3 minute song
            .Where(r => r.ConfidenceRating >= 0.85)
            .OrderByDescending(r => r.ConfidenceRating)
            .Select(r => r.Recording)
            .FirstOrDefault();

        // No match
        if (recording is null)
            return;

        mediaFile.SetArtist(recording.Artist);
        mediaFile.SetTitle(recording.Title);

        if (!string.IsNullOrWhiteSpace(recording.ArtistSort))
            mediaFile.SetArtistSort(recording.ArtistSort);

        if (!string.IsNullOrWhiteSpace(recording.Album))
            mediaFile.SetAlbum(recording.Album);
    }

    public async Task InjectTagsAsync(
        string filePath,
        IVideo video,
        CancellationToken cancellationToken = default)
    {
        using var mediaFile = MediaFile.Create(filePath);

        await InjectThumbnailAsync(mediaFile, video, cancellationToken);
        await InjectRecordingMetadataAsync(mediaFile, video, cancellationToken);

        // Inject some misc metadata
        {
            if (video is Video videoFull && !string.IsNullOrWhiteSpace(videoFull.Description))
                mediaFile.SetDescription(videoFull.Description);

            mediaFile.SetComment($"Downloaded from YouTube: {video.Url}");
        }
    }
}