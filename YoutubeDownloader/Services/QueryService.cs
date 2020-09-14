using System;
using System.Threading.Tasks;
using YoutubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Services
{
    public class QueryService
    {
        private readonly YoutubeClient _youtube = new YoutubeClient();

        private VideoId? TryParseVideoId(string query)
        {
            try
            {
                return new VideoId(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private PlaylistId? TryParsePlaylistId(string query)
        {
            try
            {
                return new PlaylistId(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private ChannelId? TryParseChannelId(string query)
        {
            try
            {
                return new ChannelId(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private UserName? TryParseUserName(string query)
        {
            try
            {
                // Only URLs
                if (!query.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !query.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return null;

                return new UserName(query);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public Query ParseQuery(string query)
        {
            query = query.Trim();

            // Playlist
            var playlistId = TryParsePlaylistId(query);
            if (playlistId != null)
            {
                return new Query(QueryType.Playlist, playlistId.Value);
            }

            // Video
            var videoId = TryParseVideoId(query);
            if (videoId != null)
            {
                return new Query(QueryType.Video, videoId.Value);
            }

            // Channel
            var channelId = TryParseChannelId(query);
            if (channelId != null)
            {
                return new Query(QueryType.Channel, channelId.Value);
            }

            // User
            var userName = TryParseUserName(query);
            if (userName != null)
            {
                return new Query(QueryType.User, userName.Value);
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
                var video = await _youtube.Videos.GetAsync(query.Value);

                return new ExecutedQuery(query, video.Title, new[] { video });
            }

            // Playlist
            if (query.Type == QueryType.Playlist)
            {
                var playlist = await _youtube.Playlists.GetAsync(query.Value);
                var videos = await _youtube.Playlists.GetVideosAsync(query.Value).BufferAsync();

                return new ExecutedQuery(query, playlist.Title, videos);
            }

            // Channel
            if (query.Type == QueryType.Channel)
            {
                var channel = await _youtube.Channels.GetAsync(query.Value);
                var videos = await _youtube.Channels.GetUploadsAsync(query.Value).BufferAsync();

                return new ExecutedQuery(query, $"Channel uploads: {channel.Title}", videos);
            }

            // User
            if (query.Type == QueryType.User)
            {
                var channel = await _youtube.Channels.GetByUserAsync(query.Value);
                var videos = await _youtube.Channels.GetUploadsAsync(channel.Id).BufferAsync();

                return new ExecutedQuery(query, $"Channel uploads: {channel.Title}", videos);
            }

            // Search
            if (query.Type == QueryType.Search)
            {
                var videos = await _youtube.Search.GetVideosAsync(query.Value).BufferAsync(200);

                return new ExecutedQuery(query, $"Search: {query.Value}", videos);
            }

            throw new ArgumentException($"Could not parse query '{query}'.", nameof(query));
        }
    }
}