using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public class VideoResolver
{
    private readonly YoutubeClient _youtube;

    public VideoResolver(YoutubeClient youtube) =>
        _youtube = youtube;

    public VideoResolver()
        : this(new YoutubeClient(Http.Client))
    {
    }

    public async Task<YoutubeQueryResult> QueryAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // Playlist
        if (PlaylistId.TryParse(query) is { } playlistId)
        {
            var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
            var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);
            return new YoutubeQueryResult(YoutubeQueryKind.Playlist, playlist.Title, videos);
        }

        // Video
        if (VideoId.TryParse(query) is { } videoId)
        {
            var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
            return new YoutubeQueryResult(YoutubeQueryKind.Video, video.Title, new[] { video });
        }

        // Channel
        if (ChannelId.TryParse(query) is { } channelId)
        {
            var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);
            return new YoutubeQueryResult(YoutubeQueryKind.Channel, $"Channel uploads: {channel.Title}", videos);
        }

        // Search
        {
            var videos = await _youtube.Search.GetVideosAsync(query, cancellationToken).CollectAsync(100);
            return new YoutubeQueryResult(YoutubeQueryKind.Search, $"Search: {query}", videos);
        }
    }

    public async Task<YoutubeQueryResult> QueryAsync(
        IReadOnlyList<string> queries,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (queries.Count == 1)
            return await QueryAsync(queries.Single(), cancellationToken);

        var videos = new List<IVideo>();

        for (var i = 0; i < queries.Count; i++)
        {
            var query = await QueryAsync(queries[i], cancellationToken);
            videos.AddRange(query.Videos);

            progress?.Report((i + 1.0) / queries.Count);
        }

        return new YoutubeQueryResult(YoutubeQueryKind.Aggregate, "Multiple queries", videos);
    }
}