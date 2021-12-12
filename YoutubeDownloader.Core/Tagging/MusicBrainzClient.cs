using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Contextual;
using Contextual.Contexts;
using JsonExtensions.Http;
using JsonExtensions.Reading;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Core.Tagging;

internal class MusicBrainzClient
{
    // 4 requests per second
    private readonly ThrottleLock _throttleLock = new(TimeSpan.FromSeconds(1.0 / 4));

    private async Task<MusicBrainzResponse?> TryGetResponseAsync(string artistHint, string titleHint)
    {
        var cancellationToken = Context.Use<CancellationContext>().Token;

        var url =
            "http://musicbrainz.org/ws/2/recording/?fmt=json&query=" +
            Uri.EscapeDataString($"artist:\"{artistHint}\" AND recording:\"{titleHint}\"");

        await _throttleLock.WaitAsync(cancellationToken);

        using var response = await Http.Client.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsJsonAsync(cancellationToken);

        var recording = json
            .GetPropertyOrNull("recordings")?
            .EnumerateArrayOrNull()?
            .FirstOrDefault();

        var artist = recording?
            .GetPropertyOrNull("artist-credit")?
            .EnumerateArrayOrNull()?
            .FirstOrDefault()
            .GetPropertyOrNull("name")?
            .GetStringOrNull();

        if (string.IsNullOrWhiteSpace(artist))
            return null;

        var title = recording?
            .GetPropertyOrNull("title")?
            .GetStringOrNull();

        if (string.IsNullOrWhiteSpace(title))
            return null;

        var album = recording?
            .GetPropertyOrNull("releases")?
            .EnumerateArrayOrNull()?
            .FirstOrDefault()
            .GetPropertyOrNull("title")?
            .GetStringOrNull();

        return new MusicBrainzResponse(artist, title, album);
    }

    public async Task<MusicBrainzResponse> GetResponseAsync(string artistHint, string titleHint) =>
        await TryGetResponseAsync(artistHint, titleHint) ??
        new MusicBrainzResponse(artistHint, titleHint, null);
}