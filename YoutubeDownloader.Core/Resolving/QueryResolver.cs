using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
}