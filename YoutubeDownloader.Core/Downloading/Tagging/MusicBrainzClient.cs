using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonExtensions.Http;
using JsonExtensions.Reading;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Core.Downloading.Tagging;

internal class MusicBrainzClient
{
    // 4 requests per second
    private readonly ThrottleLock _throttleLock = new(TimeSpan.FromSeconds(1.0 / 4));

    public async Task<MusicBrainzRecording> FindRecordingAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var url =
            "http://musicbrainz.org/ws/2/recording/?version=2&fmt=json&dismax=true&limit=1&query=" +
            Uri.EscapeDataString(query);

        await _throttleLock.WaitAsync(cancellationToken);
        var json = await Http.Client.GetJsonAsync(url, cancellationToken);

        var recording = json
            .GetPropertyOrNull("recordings")?
            .EnumerateArrayOrNull()?
            .FirstOrDefault();

        var artist = recording?
            .GetPropertyOrNull("artist-credit")?
            .EnumerateArrayOrNull()?
            .FirstOrDefault()
            .GetPropertyOrNull("name")?
            .GetNonWhiteSpaceStringOrNull();

        var artistSort = recording?
            .GetPropertyOrNull("artist-credit")?
            .EnumerateArrayOrNull()?
            .FirstOrDefault()
            .GetPropertyOrNull("artist")?
            .GetPropertyOrNull("sort-name")?
            .GetNonWhiteSpaceStringOrNull();

        var title = recording?
            .GetPropertyOrNull("title")?
            .GetNonWhiteSpaceStringOrNull();

        var album = recording?
            .GetPropertyOrNull("releases")?
            .EnumerateArrayOrNull()?
            .FirstOrDefault()
            .GetPropertyOrNull("title")?
            .GetNonWhiteSpaceStringOrNull();

        return new MusicBrainzRecording(
            artist,
            artistSort,
            title,
            album
        );
    }
}