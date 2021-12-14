using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core;

public static class YoutubeQuery
{
    public static async Task<YoutubeQueryResult> ExecuteAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // Playlist
        if (PlaylistId.TryParse(query) is { } playlistId)
        {
            var playlist = await Youtube.Client.Playlists.GetAsync(playlistId, cancellationToken);
            var videos = await Youtube.Client.Playlists.GetVideosAsync(playlistId, cancellationToken);
            return new YoutubeQueryResult(YoutubeQueryKind.Playlist, playlist.Title, videos);
        }

        // Video
        if (VideoId.TryParse(query) is { } videoId)
        {
            var video = await Youtube.Client.Videos.GetAsync(videoId, cancellationToken);
            return new YoutubeQueryResult(YoutubeQueryKind.Video, video.Title, new[] { video });
        }

        // Channel
        if (ChannelId.TryParse(query) is { } channelId)
        {
            var channel = await Youtube.Client.Channels.GetAsync(channelId, cancellationToken);
            var videos = await Youtube.Client.Channels.GetUploadsAsync(channelId, cancellationToken);
            return new YoutubeQueryResult(YoutubeQueryKind.Channel, $"Channel uploads: {channel.Title}", videos);
        }

        // Search
        {
            var videos = await Youtube.Client.Search.GetVideosAsync(query, cancellationToken).CollectAsync(100);
            return new YoutubeQueryResult(YoutubeQueryKind.Search, $"Search: {query}", videos);
        }
    }

    public static async Task<IReadOnlyList<YoutubeQueryResult>> ExecuteAsync(
        IReadOnlyList<string> queries,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new List<YoutubeQueryResult>(queries.Count);

        for (var i = 0; i < queries.Count; i++)
        {
            result.Add(await ExecuteAsync(queries[i], cancellationToken));
            progress?.Report((i + 1.0) / queries.Count);
        }

        return result;
    }
}