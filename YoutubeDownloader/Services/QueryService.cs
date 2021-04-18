using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Services
{
    public class QueryService
    {
        private readonly YoutubeClient _youtube = new();

        public Query ParseQuery(string query)
        {
            query = query.Trim();

            // Playlist
            var playlistId = PlaylistId.TryParse(query);
            if (playlistId is not null)
            {
                return new Query(QueryKind.Playlist, playlistId.Value);
            }

            // Video
            var videoId = VideoId.TryParse(query);
            if (videoId is not null)
            {
                return new Query(QueryKind.Video, videoId.Value);
            }

            // Channel
            var channelId = ChannelId.TryParse(query);
            if (channelId is not null)
            {
                return new Query(QueryKind.Channel, channelId.Value);
            }

            // Search
            {
                return new Query(QueryKind.Search, query);
            }
        }

        public IReadOnlyList<Query> ParseMultilineQuery(string query) =>
            query.Split(Environment.NewLine).Select(ParseQuery).ToArray();

        public async Task<ExecutedQuery> ExecuteQueryAsync(Query query)
        {
            // Video
            if (query.Kind == QueryKind.Video)
            {
                var video = await _youtube.Videos.GetAsync(query.Value);

                return new ExecutedQuery(query, video.Title, new[] {video});
            }

            // Playlist
            if (query.Kind == QueryKind.Playlist)
            {
                var playlist = await _youtube.Playlists.GetAsync(query.Value);
                var videos = await _youtube.Playlists.GetVideosAsync(query.Value);

                return new ExecutedQuery(query, playlist.Title, videos);
            }

            // Channel
            if (query.Kind == QueryKind.Channel)
            {
                var channel = await _youtube.Channels.GetAsync(query.Value);
                var videos = await _youtube.Channels.GetUploadsAsync(query.Value);

                return new ExecutedQuery(query, $"Channel uploads: {channel.Title}", videos);
            }

            // Search
            if (query.Kind == QueryKind.Search)
            {
                var videos = await _youtube.Search.GetVideosAsync(query.Value).CollectAsync(100);

                return new ExecutedQuery(query, $"Search: {query.Value}", videos);
            }

            throw new ArgumentException($"Could not parse query '{query}'.", nameof(query));
        }

        public async Task<IReadOnlyList<ExecutedQuery>> ExecuteQueriesAsync(
            IReadOnlyList<Query> queries,
            IProgress<double>? progress = null)
        {
            var result = new List<ExecutedQuery>(queries.Count);

            for (var i = 0; i < queries.Count; i++)
            {
                var executedQuery = await ExecuteQueryAsync(queries[i]);
                result.Add(executedQuery);

                progress?.Report((i + 1.0) / queries.Count);
            }

            return result;
        }
    }
}