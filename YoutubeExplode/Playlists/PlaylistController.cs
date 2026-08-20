using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists;

internal class PlaylistController(HttpClient http)
{
    // Works only with user-made playlists
    public async ValueTask<PlaylistBrowseResponse> GetPlaylistBrowseResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/browse"
        );

        request.Content = new StringContent(
            // lang=json
            $$"""
            {
              "browseId": {{Json.Encode("VL" + playlistId)}},
              "context": {
                "client": {
                  "clientName": "WEB",
                  "clientVersion": "2.20210408.08.00",
                  "hl": "en",
                  "gl": "US",
                  "utcOffsetMinutes": 0
                }
              }
            }
            """
        );

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playlistResponse = PlaylistBrowseResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playlistResponse.IsAvailable)
            throw new PlaylistUnavailableException($"Playlist '{playlistId}' is not available.");

        return playlistResponse;
    }

    // Works on all playlists, but contains limited metadata
    public async ValueTask<PlaylistNextResponse> GetPlaylistNextResponseAsync(
        PlaylistId playlistId,
        VideoId? videoId = null,
        int index = 0,
        string? visitorData = null,
        CancellationToken cancellationToken = default
    )
    {
        const int retriesCount = 5;
        for (var retriesRemaining = retriesCount; ; retriesRemaining--)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.youtube.com/youtubei/v1/next"
            );

            request.Content = new StringContent(
                // lang=json
                $$"""
                {
                  "playlistId": {{Json.Encode(playlistId)}},
                  "videoId": {{Json.Encode(videoId)}},
                  "playlistIndex": {{Json.Encode(index)}},
                  "context": {
                    "client": {
                      "clientName": "WEB",
                      "clientVersion": "2.20210408.08.00",
                      "hl": "en",
                      "gl": "US",
                      "utcOffsetMinutes": 0,
                      "visitorData": {{Json.Encode(visitorData)}}
                    }
                  }
                }
                """
            );

            using var response = await http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var playlistResponse = PlaylistNextResponse.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken)
            );

            if (!playlistResponse.IsAvailable)
            {
                // Playlist is unavailable, this is the first request, and we haven't retried yet.
                // Try to "open" the playlist page, because some system playlists don't actually
                // fully materialize through this endpoint until someone opens them at least once.
                if (
                    index <= 0
                    && string.IsNullOrWhiteSpace(visitorData)
                    && retriesRemaining >= retriesCount
                )
                {
                    using (
                        await http.GetAsync(
                            $"https://youtube.com/playlist?list={playlistId}",
                            cancellationToken
                        )
                    )
                    {
                        // We don't actually care about the outcome of this request
                    }

                    continue;
                }

                // Playlist is unavailable, but this is not the first request and previous requests were successful.
                // Retry because this is most likely a transient error.
                if (index > 0 && !string.IsNullOrWhiteSpace(visitorData) && retriesRemaining > 0)
                    continue;

                // Playlist is unavailable but contains videos. This might be caused by the fact that the target
                // video is unavailable, but the playlist itself is not.
                // Return the response anyway and let the call site move on.
                // https://github.com/Tyrrrz/YoutubeExplode/issues/921#issuecomment-3447937054
                if (retriesRemaining <= 0 && playlistResponse.Videos.Any())
                    return playlistResponse;

                throw new PlaylistUnavailableException(
                    $"Playlist '{playlistId}' is not available."
                );
            }

            return playlistResponse;
        }
    }

    public async ValueTask<IPlaylistData> GetPlaylistResponseAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await GetPlaylistBrowseResponseAsync(playlistId, cancellationToken);
        }
        catch (PlaylistUnavailableException)
        {
            return await GetPlaylistNextResponseAsync(playlistId, null, 0, null, cancellationToken);
        }
    }
}
