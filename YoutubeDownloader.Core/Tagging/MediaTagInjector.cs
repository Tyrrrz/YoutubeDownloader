using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using TagLib.Mpeg4;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using MediaFile = TagLib.File;

namespace YoutubeDownloader.Core.Tagging;

public class MediaTagInjector
{
    private readonly MusicBrainzClient _musicBrainz = new();

    private async Task InjectThumbnailAsync(
        IVideo video,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var thumbnail = video.Thumbnails.TryGetWithHighestResolution();
        if (thumbnail is null)
            return;

        using var response = await Http.Client.GetAsync(
            thumbnail.Url,
            HttpCompletionOption.ResponseContentRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
            return;

        var data = await response.Content.ReadAsByteArrayAsync();

        var file = MediaFile.Create(filePath);

        file.Tag.Pictures = new IPicture[]
        {
            new Picture(new ReadOnlyByteVector(data))
        };

        file.Save();
    }

    private bool TryExtractArtistAndTitle(
        string videoTitle,
        out string? artist,
        out string? title)
    {
        // Get rid of common rubbish in music video titles
        videoTitle = videoTitle.Replace("(official video)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(official lyric video)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(official music video)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(official audio)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(official)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(lyric video)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(lyrics)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(acoustic video)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(acoustic)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(live)", "", StringComparison.OrdinalIgnoreCase);
        videoTitle = videoTitle.Replace("(animated video)", "", StringComparison.OrdinalIgnoreCase);

        // Split by common artist/title separator characters
        var split = videoTitle.Split(new[] {" - ", " ~ ", " — ", " – "}, StringSplitOptions.RemoveEmptyEntries);

        // Extract artist and title
        if (split.Length >= 2)
        {
            artist = split[0].Trim();
            title = split[1].Trim();
            return true;
        }

        if (split.Length == 1)
        {
            artist = null;
            title = split[0].Trim();
            return true;
        }

        artist = null;
        title = null;
        return false;
    }

    private async Task InjectAudioTagsAsync(
        IVideo video,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        // TODO: use filename as final fallback
        if (!TryExtractArtistAndTitle(video.Title, out var artist, out var title))
            return;

        var tagsJson = await TryGetMusicBrainzTagsJsonAsync(artist!, title!, cancellationToken);

        var picture = await TryGetPictureAsync(video, cancellationToken);

        var resolvedArtist = tagsJson?["artist-credit"]?.FirstOrDefault()?["name"]?.Value<string>();
        var resolvedTitle = tagsJson?["title"]?.Value<string>();
        var resolvedAlbumName = tagsJson?["releases"]?.FirstOrDefault()?["title"]?.Value<string>();

        var file = MediaFile.Create(filePath);

        file.Tag.Performers = new[] {resolvedArtist ?? artist ?? ""};
        file.Tag.Title = resolvedTitle ?? title ?? "";
        file.Tag.Album = resolvedAlbumName ?? "";

        file.Tag.Pictures = picture is not null
            ? new[] {picture}
            : Array.Empty<IPicture>();

        file.Save();
    }

    private async Task InjectVideoTagsAsync(
        IVideo video,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var picture = await TryGetPictureAsync(video, cancellationToken);

        var file = MediaFile.Create(filePath);

        var appleTag = file.GetTag(TagTypes.Apple) as AppleTag;
        appleTag?.SetDashBox("Channel", "Channel", video.Author.Title);

        file.Tag.Pictures = picture is not null
            ? new[] {picture}
            : Array.Empty<IPicture>();

        file.Save();
    }

    public async Task InjectTagsAsync(
        IVideo video,
        string format,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: generalize formats
            if (string.Equals(format, "mp4", StringComparison.OrdinalIgnoreCase))
            {
                await InjectVideoTagsAsync(video, filePath, cancellationToken);
            }
            else if (
                string.Equals(format, "mp3", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "ogg", StringComparison.OrdinalIgnoreCase))
            {
                await InjectAudioTagsAsync(video, filePath, cancellationToken);
            }
        }
        catch
        {
            // Dont throw if tagging fails as it's not essential
        }

        // Other formats are not supported for tagging
    }
}