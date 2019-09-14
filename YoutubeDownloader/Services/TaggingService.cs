using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ATL;
using Newtonsoft.Json.Linq;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;

namespace YoutubeDownloader.Services
{
    public class TaggingService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private DateTimeOffset _lastRequestInstance = DateTimeOffset.MinValue;

        public TaggingService()
        {
            // MusicBrainz requires user agent to be set
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YoutubeDownloader (https://github.com/Tyrrrz/YoutubeDownloader)");
        }

        private async Task MaintainRateLimitAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            // Gain lock
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                // Wait until enough time has passed since last request
                var timePassedSinceLastRequest = DateTimeOffset.Now - _lastRequestInstance;
                var remainingTime = interval - timePassedSinceLastRequest;
                if (remainingTime > TimeSpan.Zero)
                    await Task.Delay(remainingTime, cancellationToken);

                _lastRequestInstance = DateTimeOffset.Now;
            }
            finally
            {
                // Release the lock
                _semaphore.Release();
            }
        }

        private async Task<JToken> TryGetTagsJsonAsync(string artist, string title, CancellationToken cancellationToken)
        {
            var url = Uri.EscapeUriString(
                "http://musicbrainz.org/ws/2/recording/?fmt=json&query=" +
                $"artist:\"{artist}\" AND recording:\"{title}\"");

            try
            {
                // 4 requests per second
                await MaintainRateLimitAsync(TimeSpan.FromSeconds(1.0 / 4), cancellationToken);

                using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                        return null;

                    var raw = await response.Content.ReadAsStringAsync();

                    return JToken.Parse(raw)["recordings"]?.FirstOrDefault();
                }
            }
            catch
            {
                return null;
            }
        }

        private bool TryExtractArtistAndTitle(string videoTitle, out string artist, out string title)
        {
            // Get rid of common rubbish in music video titles
            videoTitle = videoTitle.Replace("(official video)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(official lyric video)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(official music video)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(official audio)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(official)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(lyric video)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(lyrics)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(acoustic video)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(acoustic)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(live)", "", StringComparison.OrdinalIgnoreCase);
            videoTitle = videoTitle.Replace("(animated video)", "", StringComparison.OrdinalIgnoreCase);

            // Split by common artist/title separator characters
            var split = videoTitle.Split('-', '~', '—', '–');

            // Extract artist and title
            if (split.Length >= 2)
            {
                artist = split[0].Trim();
                title = split[1].Trim();
                return true;
            }

            if (split.Length == 1)
            {
                artist = null;
                title = split[0].Trim();
                return true;
            }

            artist = null;
            title = null;
            return false;
        }

        public async Task InjectTagsAsync(Video video, string format, string filePath, CancellationToken cancellationToken)
        {
            // Only audio files are supported at the moment
            if (!string.Equals(format, "mp3", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(format, "ogg", StringComparison.OrdinalIgnoreCase))
                return;

            // Try to extract artist/title from video title
            if (!TryExtractArtistAndTitle(video.Title, out var artist, out var title))
                return;

            // Try to get tags
            var tagsJson = await TryGetTagsJsonAsync(artist, title, cancellationToken);
            if (tagsJson == null || tagsJson["score"].Value<int>() < 50)
                return;

            // Extract information
            var resolvedArtist = tagsJson["artist-credit"]?.FirstOrDefault()?["name"]?.Value<string>();
            var resolvedTitle = tagsJson["title"]?.Value<string>();
            var resolvedAlbumName = tagsJson["releases"]?.FirstOrDefault()?["title"]?.Value<string>();

            // Inject tags
            new Track(filePath)
            {
                Artist = resolvedArtist ?? artist ?? "",
                Title = resolvedTitle ?? title ?? "",
                Album = resolvedAlbumName ?? "",
                DurationMs = video.Duration.TotalMilliseconds
            }.Save();
        }
    }
}