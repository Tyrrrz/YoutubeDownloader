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
        // Use MusicBrainz to automatically resolve tags based on video title
        var recording = await _musicBrainz.FindRecordingAsync(video.Title, cancellationToken);

        if (!string.IsNullOrWhiteSpace(recording.Artist))
            mediaFile.SetArtist(recording.Artist);

        if (!string.IsNullOrWhiteSpace(recording.ArtistSort))
            mediaFile.SetArtistSort(recording.ArtistSort);

        if (!string.IsNullOrWhiteSpace(recording.Title))
            mediaFile.SetTitle(recording.Title);

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