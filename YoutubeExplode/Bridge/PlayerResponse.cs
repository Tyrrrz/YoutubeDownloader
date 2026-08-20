using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonExtensions.Reading;
using Lazy;
using PowerKit.Extensions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge;

internal partial class PlayerResponse(JsonElement content)
{
    [Lazy]
    private JsonElement? Playability => content.GetPropertyOrNull("playabilityStatus");

    [Lazy]
    private string? PlayabilityStatus =>
        Playability?.GetPropertyOrNull("status")?.GetStringOrNull();

    [Lazy]
    public string? PlayabilityError => Playability?.GetPropertyOrNull("reason")?.GetStringOrNull();

    [Lazy]
    public bool IsAvailable =>
        !string.Equals(PlayabilityStatus, "error", StringComparison.OrdinalIgnoreCase)
        && Details is not null;

    [Lazy]
    public bool IsPlayable =>
        string.Equals(PlayabilityStatus, "ok", StringComparison.OrdinalIgnoreCase);

    [Lazy]
    private JsonElement? Details => content.GetPropertyOrNull("videoDetails");

    [Lazy]
    public string? Title => Details?.GetPropertyOrNull("title")?.GetStringOrNull();

    [Lazy]
    public string? ChannelId => Details?.GetPropertyOrNull("channelId")?.GetStringOrNull();

    [Lazy]
    public string? Author => Details?.GetPropertyOrNull("author")?.GetStringOrNull();

    [Lazy]
    public DateTimeOffset? UploadDate =>
        content
            .GetPropertyOrNull("microformat")
            ?.GetPropertyOrNull("playerMicroformatRenderer")
            ?.GetPropertyOrNull("uploadDate")
            ?.GetDateTimeOffset();

    [Lazy]
    public TimeSpan? Duration =>
        Details
            ?.GetPropertyOrNull("lengthSeconds")
            ?.GetStringOrNull()
            ?.Pipe(s => double.ParseOrNull(s, CultureInfo.InvariantCulture))
            ?.Pipe(TimeSpan.FromSeconds);

    [Lazy]
    public IReadOnlyList<ThumbnailData> Thumbnails =>
        Details
            ?.GetPropertyOrNull("thumbnail")
            ?.GetPropertyOrNull("thumbnails")
            ?.EnumerateArrayOrNull()
            ?.Select(j => new ThumbnailData(j))
            .ToArray()
        ?? [];

    public IReadOnlyList<string> Keywords =>
        Details
            ?.GetPropertyOrNull("keywords")
            ?.EnumerateArrayOrNull()
            ?.Select(j => j.GetStringOrNull())
            .WhereNotNull()
            .ToArray()
        ?? [];

    [Lazy]
    public string? Description => Details?.GetPropertyOrNull("shortDescription")?.GetStringOrNull();

    [Lazy]
    public long? ViewCount =>
        Details
            ?.GetPropertyOrNull("viewCount")
            ?.GetStringOrNull()
            ?.Pipe(s => long.ParseOrNull(s, CultureInfo.InvariantCulture));

    [Lazy]
    public string? PreviewVideoId =>
        Playability
            ?.GetPropertyOrNull("errorScreen")
            ?.GetPropertyOrNull("playerLegacyDesktopYpcTrailerRenderer")
            ?.GetPropertyOrNull("trailerVideoId")
            ?.GetStringOrNull()
        ?? Playability
            ?.GetPropertyOrNull("errorScreen")
            ?.GetPropertyOrNull("ypcTrailerRenderer")
            ?.GetPropertyOrNull("playerVars")
            ?.GetStringOrNull()
            ?.Pipe(UrlEx.GetQueryParameters)
            .GetValueOrDefault("video_id")
        ?? Playability
            ?.GetPropertyOrNull("errorScreen")
            ?.GetPropertyOrNull("ypcTrailerRenderer")
            ?.GetPropertyOrNull("playerResponse")
            ?.GetStringOrNull()
            ?
            // YouTube uses weird base64-like encoding here that I don't know how to deal with.
            // It's supposed to have JSON inside, but if extracted as is, it contains garbage.
            // Luckily, some of the text gets decoded correctly, which is enough for us to
            // extract the preview video ID using regex.
            .Replace('-', '+')
            .Replace('_', '/')
            .Pipe(Convert.FromBase64String)
            .Pipe(Encoding.UTF8.GetString)
            .Pipe(s => Regex.Match(s, @"video_id=(.{11})").Groups[1].Value)
            .NullIfWhiteSpace();

    [Lazy]
    private JsonElement? StreamingData => content.GetPropertyOrNull("streamingData");

    [Lazy]
    public string? DashManifestUrl =>
        StreamingData?.GetPropertyOrNull("dashManifestUrl")?.GetStringOrNull();

    [Lazy]
    public string? HlsManifestUrl =>
        StreamingData?.GetPropertyOrNull("hlsManifestUrl")?.GetStringOrNull();

    [Lazy]
    public IReadOnlyList<IStreamData> Streams
    {
        get
        {
            var result = new List<IStreamData>();

            var muxedStreams = StreamingData
                ?.GetPropertyOrNull("formats")
                ?.EnumerateArrayOrNull()
                ?.Select(j => new StreamData(j));

            if (muxedStreams is not null)
                result.AddRange(muxedStreams);

            var adaptiveStreams = StreamingData
                ?.GetPropertyOrNull("adaptiveFormats")
                ?.EnumerateArrayOrNull()
                ?.Select(j => new StreamData(j));

            if (adaptiveStreams is not null)
                result.AddRange(adaptiveStreams);

            return result;
        }
    }

    [Lazy]
    public IReadOnlyList<ClosedCaptionTrackData> ClosedCaptionTracks =>
        content
            .GetPropertyOrNull("captions")
            ?.GetPropertyOrNull("playerCaptionsTracklistRenderer")
            ?.GetPropertyOrNull("captionTracks")
            ?.EnumerateArrayOrNull()
            ?.Select(j => new ClosedCaptionTrackData(j))
            .ToArray()
        ?? [];
}

internal partial class PlayerResponse
{
    public class ClosedCaptionTrackData(JsonElement content)
    {
        [Lazy]
        public string? Url => content.GetPropertyOrNull("baseUrl")?.GetStringOrNull();

        [Lazy]
        public string? LanguageCode => content.GetPropertyOrNull("languageCode")?.GetStringOrNull();

        [Lazy]
        public string? LanguageName =>
            content.GetPropertyOrNull("name")?.GetPropertyOrNull("simpleText")?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("name")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .Pipe(string.Concat);

        [Lazy]
        public bool IsAutoGenerated =>
            content
                .GetPropertyOrNull("vssId")
                ?.GetStringOrNull()
                ?.StartsWith("a.", StringComparison.OrdinalIgnoreCase)
            ?? false;
    }
}

internal partial class PlayerResponse
{
    public class StreamData(JsonElement content) : IStreamData
    {
        [Lazy]
        public int? Itag => content.GetPropertyOrNull("itag")?.GetInt32OrNull();

        [Lazy]
        private IReadOnlyDictionary<string, string>? CipherData =>
            content.GetPropertyOrNull("cipher")?.GetStringOrNull()?.Pipe(UrlEx.GetQueryParameters)
            ?? content
                .GetPropertyOrNull("signatureCipher")
                ?.GetStringOrNull()
                ?.Pipe(UrlEx.GetQueryParameters);

        [Lazy]
        public string? Url =>
            content.GetPropertyOrNull("url")?.GetStringOrNull()
            ?? CipherData?.GetValueOrDefault("url");

        [Lazy]
        public string? Signature => CipherData?.GetValueOrDefault("s");

        [Lazy]
        public string? SignatureParameter => CipherData?.GetValueOrDefault("sp");

        [Lazy]
        public long? ContentLength =>
            content
                .GetPropertyOrNull("contentLength")
                ?.GetStringOrNull()
                ?.Pipe(s => long.ParseOrNull(s, CultureInfo.InvariantCulture))
            ?? Url?.Pipe(s => UrlEx.TryGetQueryParameterValue(s, "clen"))
                ?.NullIfWhiteSpace()
                ?.Pipe(s => long.ParseOrNull(s, CultureInfo.InvariantCulture));

        [Lazy]
        public long? Bitrate => content.GetPropertyOrNull("bitrate")?.GetInt64OrNull();

        [Lazy]
        private string? MimeType => content.GetPropertyOrNull("mimeType")?.GetStringOrNull();

        [Lazy]
        public string? Container => MimeType?.SubstringUntil(";").SubstringAfter("/");

        [Lazy]
        private bool IsAudioOnly =>
            MimeType?.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ?? false;

        [Lazy]
        public string? Codecs => MimeType?.SubstringAfter("codecs=\"").SubstringUntil("\"");

        [Lazy]
        public string? AudioCodec =>
            IsAudioOnly ? Codecs : Codecs?.SubstringAfter(", ").NullIfWhiteSpace();

        [Lazy]
        public string? AudioLanguageCode =>
            content
                .GetPropertyOrNull("audioTrack")
                ?.GetPropertyOrNull("id")
                ?.GetStringOrNull()
                ?.SubstringUntil(".");

        [Lazy]
        public string? AudioLanguageName =>
            content
                .GetPropertyOrNull("audioTrack")
                ?.GetPropertyOrNull("displayName")
                ?.GetStringOrNull();

        [Lazy]
        public bool? IsAudioLanguageDefault =>
            content
                .GetPropertyOrNull("audioTrack")
                ?.GetPropertyOrNull("audioIsDefault")
                ?.GetBooleanOrNull();

        [Lazy]
        public string? VideoCodec
        {
            get
            {
                var codec = IsAudioOnly ? null : Codecs?.SubstringUntil(", ").NullIfWhiteSpace();

                // "unknown" value indicates av01 codec
                if (string.Equals(codec, "unknown", StringComparison.OrdinalIgnoreCase))
                    return "av01.0.05M.08";

                return codec;
            }
        }

        [Lazy]
        public string? VideoQualityLabel =>
            content.GetPropertyOrNull("qualityLabel")?.GetStringOrNull();

        [Lazy]
        public int? VideoWidth => content.GetPropertyOrNull("width")?.GetInt32OrNull();

        [Lazy]
        public int? VideoHeight => content.GetPropertyOrNull("height")?.GetInt32OrNull();

        [Lazy]
        public bool IsVideoUpscaled =>
            // xtags is a base64-encoded protobuf map<string, string>.
            // Streams upscaled with YouTube's Super Resolution feature carry the entry {"sr": "1"}.
            content
                .GetPropertyOrNull("xtags")
                ?.GetStringOrNull()
                ?.NullIfWhiteSpace()
                ?.Pipe(Protobuf.TryDeserializeMap)
                ?.GetValueOrDefault("sr") == "1";

        [Lazy]
        public int? VideoFramerate => content.GetPropertyOrNull("fps")?.GetInt32OrNull();
    }
}

internal partial class PlayerResponse
{
    public static PlayerResponse Parse(string raw) => new(Json.Parse(raw));
}
