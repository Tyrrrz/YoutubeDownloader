using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Tagging;

public class MediaTagInjector
{
    private readonly MusicBrainzClient _musicBrainz = new();

    private void InjectMiscMetadata(MediaFile mediaFile, IVideo video)
    {
        var description = (video as Video)?.Description;
        if (!string.IsNullOrWhiteSpace(description))
            mediaFile.SetDescription(description);

        mediaFile.SetComment(
            "Downloaded using YoutubeDownloader (https://github.com/Tyrrrz/YoutubeDownloader)" +
            Environment.NewLine +
            $"Video: {video.Title}" +
            Environment.NewLine +
            $"Video URL: {video.Url}" +
            Environment.NewLine +
            $"Channel: {video.Author.ChannelTitle}" +
            Environment.NewLine +
            $"Channel URL: {video.Author.ChannelUrl}"
        );
    }

    private async Task InjectMusicMetadataAsync(
        MediaFile mediaFile,
        IVideo video,
        CancellationToken cancellationToken = default)
    {
        var recordings = await _musicBrainz.SearchRecordingsAsync(video.Title, cancellationToken);

        var recording = recordings
            .FirstOrDefault(r =>
                // Recording title must be part of the video title.
                // Recording artist must be part of the video title or channel title.
                video.Title.Contains(r.Title, StringComparison.OrdinalIgnoreCase) && (
                    video.Title.Contains(r.Artist, StringComparison.OrdinalIgnoreCase) ||
                    video.Author.ChannelTitle.Contains(r.Artist, StringComparison.OrdinalIgnoreCase)
                )
            );

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
                .Where(t => string.Equals(
                    t.TryGetImageFormat(),
                    "jpg",
                    StringComparison.OrdinalIgnoreCase
                ))
                .OrderByDescending(t => t.Resolution.Area)
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