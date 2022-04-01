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

    public async Task<QueryResult> QueryAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // Playlist
        if (PlaylistId.TryParse(query) is { } playlistId)
        {
            var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
            var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);
            return new QueryResult(QueryKind.Playlist, playlist.Title, videos);
        }

        // Video
        if (VideoId.TryParse(query) is { } videoId)
        {
            var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
            return new QueryResult(QueryKind.Video, video.Title, new[] { video });
        }

        // Channel
        if (ChannelId.TryParse(query) is { } channelId)
        {
            var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);
            return new QueryResult(QueryKind.Channel, $"Channel uploads: {channel.Title}", videos);
        }

        // Search
        {
            var videos = await _youtube.Search.GetVideosAsync(query, cancellationToken).CollectAsync(100);
            return new QueryResult(QueryKind.Search, $"Search: {query}", videos);
        }
    }

    public async Task<QueryResult> QueryAsync(
        IReadOnlyList<string> queries,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Avoid wrapping results in an aggregated query if there is no need for it
        if (queries.Count == 1)
            return await QueryAsync(queries.Single(), cancellationToken);

        var videos = new List<IVideo>();
        var videoIds = new HashSet<VideoId>();

        var completedQueriesCount = 0;
        foreach (var query in queries)
        {
            var result = await QueryAsync(query, cancellationToken);

            videos.AddRange(
                result.Videos.Where(v => videoIds.Add(v.Id))
            );

            progress?.Report(Percentage.FromFraction(
                1.0 * completedQueriesCount++ / queries.Count
            ));
        }

        return new QueryResult(QueryKind.Aggregate, "Multiple queries", videos);
    }
}