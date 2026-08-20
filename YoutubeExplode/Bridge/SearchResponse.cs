using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using JsonExtensions.Reading;
using Lazy;
using PowerKit.Extensions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class SearchResponse(JsonElement content)
{
    // Search response is incredibly inconsistent (with at least 5 variations),
    // so we employ descendant searching, which is inefficient but resilient.

    [Lazy]
    private JsonElement? ContentRoot =>
        content.GetPropertyOrNull("contents")
        ?? content.GetPropertyOrNull("onResponseReceivedCommands");

    [Lazy]
    public IReadOnlyList<VideoData> Videos =>
        ContentRoot
            ?.EnumerateDescendantProperties("videoRenderer")
            .Select(j => new VideoData(j))
            .ToArray()
        ?? [];

    [Lazy]
    public IReadOnlyList<PlaylistData> Playlists =>
        ContentRoot
            ?.EnumerateDescendantProperties("lockupViewModel")
            .Select(j => new PlaylistData(j))
            .ToArray()
        ?? ContentRoot
            ?.EnumerateDescendantProperties("playlistRenderer")
            .Select(j => new PlaylistData(j))
            .ToArray()
        ?? [];

    [Lazy]
    public IReadOnlyList<ChannelData> Channels =>
        ContentRoot
            ?.EnumerateDescendantProperties("channelRenderer")
            .Select(j => new ChannelData(j))
            .ToArray()
        ?? [];

    [Lazy]
    public string? ContinuationToken =>
        ContentRoot
            ?.EnumerateDescendantProperties("continuationCommand")
            .FirstOrNull()
            ?.GetPropertyOrNull("token")
            ?.GetStringOrNull();
}

internal partial class SearchResponse
{
    internal class VideoData(JsonElement content)
    {
        [Lazy]
        public string? Id => content.GetPropertyOrNull("videoId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
            content.GetPropertyOrNull("title")?.GetPropertyOrNull("simpleText")?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("title")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .Pipe(string.Concat);

        [Lazy]
        private JsonElement? AuthorDetails =>
            content
                .GetPropertyOrNull("longBylineText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0)
            ?? content
                .GetPropertyOrNull("shortBylineText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0);

        [Lazy]
        public string? Author => AuthorDetails?.GetPropertyOrNull("text")?.GetStringOrNull();

        [Lazy]
        public string? ChannelId =>
            AuthorDetails
                ?.GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("channelThumbnailSupportedRenderers")
                ?.GetPropertyOrNull("channelThumbnailWithLinkRenderer")
                ?.GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetStringOrNull();

        [Lazy]
        public TimeSpan? Duration =>
            content
                .GetPropertyOrNull("lengthText")
                ?.GetPropertyOrNull("simpleText")
                ?.GetStringOrNull()
                ?.Pipe(s =>
                    TimeSpan.TryParseExact(
                        s,
                        [@"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss"],
                        CultureInfo.InvariantCulture,
                        out var result
                    )
                        ? result
                        : (TimeSpan?)null
                )
            ?? content
                .GetPropertyOrNull("lengthText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .Pipe(string.Concat)
                ?.Pipe(s =>
                    TimeSpan.TryParseExact(
                        s,
                        [@"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss"],
                        CultureInfo.InvariantCulture,
                        out var result
                    )
                        ? result
                        : (TimeSpan?)null
                );

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails =>
            content
                .GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("thumbnails")
                ?.EnumerateArrayOrNull()
                ?.Select(j => new ThumbnailData(j))
                .ToArray()
            ?? [];
    }
}

internal partial class SearchResponse
{
    public class PlaylistData(JsonElement content)
    {
        [Lazy]
        public string? Id =>
            content.GetPropertyOrNull("contentId")?.GetStringOrNull()
            ?? content.GetPropertyOrNull("playlistId")?.GetStringOrNull();

        [Lazy]
        private JsonElement? Metadata =>
            content.GetPropertyOrNull("metadata")?.GetPropertyOrNull("lockupMetadataViewModel");

        [Lazy]
        public string? Title =>
            Metadata?.GetPropertyOrNull("title")?.GetPropertyOrNull("content")?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("title")
                ?.GetPropertyOrNull("simpleText")
                ?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("title")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .Pipe(string.Concat);

        [Lazy]
        private JsonElement? AuthorDetails =>
            Metadata
                ?.EnumerateDescendantProperties("metadataParts")
                ?.ElementAtOrNull(0)
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0)
                ?.GetPropertyOrNull("text")
            ?? content
                .GetPropertyOrNull("longBylineText")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0);

        [Lazy]
        public string? Author =>
            AuthorDetails?.GetPropertyOrNull("content")?.GetStringOrNull()
            ?? AuthorDetails?.GetPropertyOrNull("text")?.GetStringOrNull();

        [Lazy]
        public string? ChannelId =>
            AuthorDetails
                ?.GetPropertyOrNull("commandRuns")
                ?.EnumerateArrayOrNull()
                ?.ElementAtOrNull(0)
                ?.GetPropertyOrNull("onTap")
                ?.GetPropertyOrNull("innertubeCommand")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetStringOrNull()
            ?? AuthorDetails
                ?.GetPropertyOrNull("navigationEndpoint")
                ?.GetPropertyOrNull("browseEndpoint")
                ?.GetPropertyOrNull("browseId")
                ?.GetStringOrNull();

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails =>
            content
                .GetPropertyOrNull("contentImage")
                ?.GetPropertyOrNull("collectionThumbnailViewModel")
                ?.GetPropertyOrNull("primaryThumbnail")
                ?.GetPropertyOrNull("thumbnailViewModel")
                ?.GetPropertyOrNull("image")
                ?.GetPropertyOrNull("sources")
                ?.EnumerateArrayOrEmpty()
                .Select(j => new ThumbnailData(j))
                .ToArray()
            ?? content
                .GetPropertyOrNull("thumbnails")
                ?.EnumerateDescendantProperties("thumbnails")
                .SelectMany(j => j.EnumerateArrayOrEmpty())
                .Select(j => new ThumbnailData(j))
                .ToArray()
            ?? [];
    }
}

internal partial class SearchResponse
{
    public class ChannelData(JsonElement content)
    {
        [Lazy]
        public string? Id => content.GetPropertyOrNull("channelId")?.GetStringOrNull();

        [Lazy]
        public string? Title =>
            content.GetPropertyOrNull("title")?.GetPropertyOrNull("simpleText")?.GetStringOrNull()
            ?? content
                .GetPropertyOrNull("title")
                ?.GetPropertyOrNull("runs")
                ?.EnumerateArrayOrNull()
                ?.Select(j => j.GetPropertyOrNull("text")?.GetStringOrNull())
                .WhereNotNull()
                .Pipe(string.Concat);

        [Lazy]
        public IReadOnlyList<ThumbnailData> Thumbnails =>
            content
                .GetPropertyOrNull("thumbnail")
                ?.GetPropertyOrNull("thumbnails")
                ?.EnumerateArrayOrNull()
                ?.Select(j => new ThumbnailData(j))
                .ToArray()
            ?? [];
    }
}

internal partial class SearchResponse
{
    public static SearchResponse Parse(string raw) => new(Json.Parse(raw));
}
