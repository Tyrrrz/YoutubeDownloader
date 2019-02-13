using System;
using System.Threading.Tasks;
using YoutubeDownloader.Models;
using YoutubeExplode;

namespace YoutubeDownloader.Services
{
    public class QueryService
    {
        private readonly IYoutubeClient _youtubeClient = new YoutubeClient();

        public Query ParseQuery(string query)
        {
            query = query.Trim();

            // Playlist ID
            if (YoutubeClient.ValidatePlaylistId(query))
            {
                return new Query(QueryType.Playlist, query);
            }

            // Playlist URL
            if (YoutubeClient.TryParsePlaylistId(query, out var playlistId))
            {
                return new Query(QueryType.Playlist, playlistId);
            }

            // Video ID
            if (YoutubeClient.ValidateVideoId(query))
            {
                return new Query(QueryType.Video, query);
            }

            // Video URL
            if (YoutubeClient.TryParseVideoId(query, out var videoId))
            {
                return new Query(QueryType.Video, videoId);
            }

            // Channel ID
            if (YoutubeClient.TryParseChannelId(query, out var channelId))
            {
                return new Query(QueryType.Channel, channelId);
            }

            // Search
            {
                return new Query(QueryType.Search, query);
            }
        }

        public async Task<ExecutedQuery> ExecuteQueryAsync(Query query)
        {
            // Video
            if (query.Type == QueryType.Video)
            {
                var video = await _youtubeClient.GetVideoAsync(query.Value);
                var title = video.Title;

                return new ExecutedQuery(query, title, new[] { video });
            }

            // Playlist
            if (query.Type == QueryType.Playlist)
            {
                var playlist = await _youtubeClient.GetPlaylistAsync(query.Value);
                var title = playlist.Title;

                return new ExecutedQuery(query, title, playlist.Videos);
            }

            // Channel
            if (query.Type == QueryType.Channel)
            {
                var videos = await _youtubeClient.GetChannelUploadsAsync(query.Value);
                var title = $"Channel uploads";

                return new ExecutedQuery(query, title, videos);
            }

            // Search
            if (query.Type == QueryType.Search)
            {
                var videos = await _youtubeClient.SearchVideosAsync(query.Value, 5);
                var title = $"Search: {query.Value}";

                return new ExecutedQuery(query, title, videos);
            }

            throw new ArgumentException($@"Could not parse query [{query}].", nameof(query));
        }

        public Task<ExecutedQuery> ExecuteQueryAsync(string query)
            => ExecuteQueryAsync(ParseQuery(query));
    }
}