using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using JsonExtensions.Reading;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos;

internal class VideoController(HttpClient http)
{
    private string? _visitorData;

    protected HttpClient Http { get; } = http;

    private async ValueTask<string> ResolveVisitorDataAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (!string.IsNullOrWhiteSpace(_visitorData))
            return _visitorData;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://www.youtube.com/sw.js_data"
        );

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        request.Headers.Add(
            "User-Agent",
            "com.google.android.youtube/20.10.38 (Linux; U; ANDROID 11) gzip"
        );

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        // TODO: move this to a bridge wrapper
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (jsonString.StartsWith(")]}'"))
            jsonString = jsonString[4..];

        var json = Json.Parse(jsonString);

        // This is just an ordered (but unstructured) blob of data
        var value = json[0][2][0][0][13].GetStringOrNull();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new YoutubeExplodeException("Failed to resolve visitor data.");
        }

        return _visitorData = value;
    }

    public async ValueTask<VideoWatchPage> GetVideoWatchPageAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            var watchPage = VideoWatchPage.TryParse(
                await Http.GetStringAsync(
                    $"https://www.youtube.com/watch?v={videoId}&bpctr=9999999999",
                    cancellationToken
                )
            );

            if (watchPage is null)
            {
                if (retriesRemaining > 0)
                    continue;

                throw new YoutubeExplodeException(
                    "Video watch page is broken. Please try again in a few minutes."
                );
            }

            if (!watchPage.IsAvailable)
                throw new VideoUnavailableException($"Video '{videoId}' is not available.");

            return watchPage;
        }
    }

    private async ValueTask<PlayerResponse> SendPlayerRequestAsync(
        string content,
        string userAgent,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player"
        );

        request.Content = new StringContent(content);
        request.Headers.Add("User-Agent", userAgent);

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return PlayerResponse.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async ValueTask<PlayerResponse> GetPlayerResponseAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        var visitorData = await ResolveVisitorDataAsync(cancellationToken);

        // YouTube now requires Proof of Origin (PO) tokens for most Innertube clients
        // (WEB, iOS, ANDROID, ANDROID_VR, etc.), causing stream downloads to fail with
        // 403 Forbidden or LOGIN_REQUIRED bot-check errors.
        // VISIONOS currently returns progressive streams without PO tokens or deciphering,
        // matching yt-dlp's default JS-less client.
        var playerResponse = await SendPlayerRequestAsync(
            // lang=json
            $$"""
            {
              "videoId": {{Json.Encode(videoId)}},
              "contentCheckOk": true,
              "racyCheckOk": true,
              "context": {
                "client": {
                  "clientName": "VISIONOS",
                  "clientVersion": "1.02",
                  "deviceMake": "Apple",
                  "deviceModel": "RealityDevice17,1",
                  "osName": "visionOS",
                  "osVersion": "26.5.23O471",
                  "visitorData": {{Json.Encode(visitorData)}},
                  "hl": "en",
                  "gl": "US",
                  "utcOffsetMinutes": 0
                }
              }
            }
            """,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 15_7_3) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Safari/605.1.15",
            cancellationToken
        );

        if (playerResponse.IsPlayable)
            return playerResponse;

        // "Made for kids" videos are not available on VISIONOS (or ANDROID_VR).
        // ANDROID still returns a muxed itag-18 stream without deciphering.
        var androidResponse = await SendPlayerRequestAsync(
            // lang=json
            $$"""
            {
              "videoId": {{Json.Encode(videoId)}},
              "contentCheckOk": true,
              "racyCheckOk": true,
              "context": {
                "client": {
                  "clientName": "ANDROID",
                  "clientVersion": "21.26.364",
                  "androidSdkVersion": 30,
                  "osName": "Android",
                  "osVersion": "11",
                  "visitorData": {{Json.Encode(visitorData)}},
                  "hl": "en",
                  "gl": "US",
                  "utcOffsetMinutes": 0
                }
              }
            }
            """,
            "com.google.android.youtube/21.26.364 (Linux; U; Android 11) gzip",
            cancellationToken
        );

        if (androidResponse.IsPlayable)
            return androidResponse;

        if (!playerResponse.IsAvailable && !androidResponse.IsAvailable)
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        return playerResponse.IsAvailable ? playerResponse : androidResponse;
    }

    public async ValueTask<PlayerResponse> GetPlayerResponseAsync(
        VideoId videoId,
        string? signatureTimestamp,
        CancellationToken cancellationToken = default
    )
    {
        var visitorData = await ResolveVisitorDataAsync(cancellationToken);

        // The only client that can handle age-restricted videos without authentication is the
        // TVHTML5_SIMPLY_EMBEDDED_PLAYER client.
        // This client does require signature deciphering, so we only use it as a fallback.
        var playerResponse = await SendPlayerRequestAsync(
            // lang=json
            $$"""
            {
              "videoId": {{Json.Encode(videoId)}},
              "context": {
                "client": {
                  "clientName": "TVHTML5_SIMPLY_EMBEDDED_PLAYER",
                  "clientVersion": "2.0",
                  "visitorData": {{Json.Encode(visitorData)}},
                  "hl": "en",
                  "gl": "US",
                  "utcOffsetMinutes": 0
                },
                "thirdParty": {
                  "embedUrl": "https://www.youtube.com"
                }
              },
              "playbackContext": {
                "contentPlaybackContext": {
                  "signatureTimestamp": {{Json.Encode(signatureTimestamp)}}
                }
              }
            }
            """,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36",
            cancellationToken
        );

        if (!playerResponse.IsAvailable)
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        return playerResponse;
    }
}
