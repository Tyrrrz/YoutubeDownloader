using System;
using System.Collections.Generic;
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

    public async Task<IReadOnlyList<IVideo>> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // Playlist
        if (PlaylistId.TryParse(query) is { } playlistId)
        {
            return await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);
        }

        // Video
        if (VideoId.TryParse(query) is { } videoId)
        {
            return new[] {await _youtube.Videos.GetAsync(videoId, cancellationToken)};
        }

        // Channel
        if (ChannelId.TryParse(query) is { } channelId)
        {
            return await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);
        }

        // Search
        {
            return await _youtube.Search.GetVideosAsync(query, cancellationToken).CollectAsync(100);
        }
    }

    public async Task<IReadOnlyList<IVideo>> ResolveAsync(
        IReadOnlyList<string> queries,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var videos = new List<IVideo>();
        var completed = 0;

        foreach (var query in queries)
        {
            videos.AddRange(
                await ResolveAsync(query, cancellationToken)
            );

            progress?.Report(
                Percentage.FromFraction(1.0 * ++completed / queries.Count)
            );
        }

        return videos;
    }
}