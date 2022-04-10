using System;
using System.Collections.Generic;
using System.Linq;
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

public class QueryResolver
{
    private readonly YoutubeClient _youtube = new(Http.Client);

    public async Task<QueryResult> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // Playlist
        if (PlaylistId.TryParse(query) is { } playlistId)
        {
            var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
            var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);
            return new QueryResult(QueryResultKind.Playlist, playlist.Title, videos);
        }

        // Video
        if (VideoId.TryParse(query) is { } videoId)
        {
            var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
            return new QueryResult(QueryResultKind.Video, video.Title, new[] {video});
        }

        // Channel
        if (ChannelId.TryParse(query) is { } channelId)
        {
            var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);
            return new QueryResult(QueryResultKind.Channel, channel.Title, videos);
        }

        // Search
        {
            var videos = await _youtube.Search.GetVideosAsync(query, cancellationToken).CollectAsync(20);
            return new QueryResult(QueryResultKind.Search, $"Search: {query}", videos);
        }
    }

    public async Task<QueryResult> ResolveAsync(
        IReadOnlyList<string> queries,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (queries.Count == 1)
            return await ResolveAsync(queries.Single(), cancellationToken);

        var videos = new List<IVideo>();
        var completed = 0;

        foreach (var query in queries)
        {
            var result = await ResolveAsync(query, cancellationToken);
            videos.AddRange(result.Videos);

            progress?.Report(
                Percentage.FromFraction(1.0 * ++completed / queries.Count)
            );
        }

        return new QueryResult(QueryResultKind.Aggregate, "Multiple queries", videos);
    }
}