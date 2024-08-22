using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public class QueryResolver(IReadOnlyList<Cookie>? initialCookies = null)
{
    private readonly YoutubeClient _youtube = new(Http.Client, initialCookies ?? []);
    private readonly bool _isAuthenticated = initialCookies?.Any() == true;

    private async Task<QueryResult?> TryResolvePlaylistAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (PlaylistId.TryParse(query) is not { } playlistId)
            return null;

        // Skip personal system playlists if the user is not authenticated
        var isPersonalSystemPlaylist =
            playlistId == "WL" || playlistId == "LL" || playlistId == "LM";

        if (isPersonalSystemPlaylist && !_isAuthenticated)
            return null;

        var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
        var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);

        return new QueryResult(QueryResultKind.Playlist, $"Playlist: {playlist.Title}", videos);
    }

    private async Task<QueryResult?> TryResolveVideoAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (VideoId.TryParse(query) is not { } videoId)
            return null;

        var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
        return new QueryResult(QueryResultKind.Video, video.Title, [video]);
    }

    private async Task<QueryResult?> TryResolveChannelAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (ChannelId.TryParse(query) is { } channelId)
        {
            var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (ChannelHandle.TryParse(query) is { } channelHandle)
        {
            var channel = await _youtube.Channels.GetByHandleAsync(
                channelHandle,
                cancellationToken
            );

            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (UserName.TryParse(query) is { } userName)
        {
            var channel = await _youtube.Channels.GetByUserAsync(userName, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (ChannelSlug.TryParse(query) is { } channelSlug)
        {
            var channel = await _youtube.Channels.GetBySlugAsync(channelSlug, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        return null;
    }

    private async Task<QueryResult> ResolveSearchAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        var videos = await _youtube
            .Search.GetVideosAsync(query, cancellationToken)
            .CollectAsync(20);

        return new QueryResult(QueryResultKind.Search, $"Search: {query}", videos);
    }

    public async Task<QueryResult> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        // If the query starts with a question mark, it's always treated as a search query
        if (query.StartsWith('?'))
            return await ResolveSearchAsync(query[1..], cancellationToken);

        return await TryResolvePlaylistAsync(query, cancellationToken)
            ?? await TryResolveVideoAsync(query, cancellationToken)
            ?? await TryResolveChannelAsync(query, cancellationToken)
            ?? await ResolveSearchAsync(query, cancellationToken);
    }

    public async Task<QueryResult> ResolveAsync(
        IReadOnlyList<string> queries,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        if (queries.Count == 1)
            return await ResolveAsync(queries.Single(), cancellationToken);

        var videos = new List<IVideo>();
        var videoIds = new HashSet<VideoId>();

        var completed = 0;
        foreach (var query in queries)
        {
            var result = await ResolveAsync(query, cancellationToken);

            foreach (var video in result.Videos)
            {
                if (videoIds.Add(video.Id))
                    videos.Add(video);
            }

            progress?.Report(Percentage.FromFraction(1.0 * ++completed / queries.Count));
        }

        return new QueryResult(QueryResultKind.Aggregate, $"{queries.Count} queries", videos);
    }
}
