using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TagLib;
using TagLib.Mpeg4;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using File = TagLib.File;

namespace YoutubeDownloader.Services
{
    public class TaggingService
    {
        private readonly HttpClient _httpClient = new();

        private readonly SemaphoreSlim _requestRateSemaphore = new(1, 1);
        private DateTimeOffset _lastRequestInstant = DateTimeOffset.MinValue;

        public TaggingService()
        {
            // MusicBrainz requires user agent to be set
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
        }

        private async Task MaintainRateLimitAsync(
            TimeSpan interval,
            CancellationToken cancellationToken =default)
        {
            // Gain lock
            await _requestRateSemaphore.WaitAsync(cancellationToken);

            try
            {
                // Wait until enough time has passed since last request
                var timePassedSinceLastRequest = DateTimeOffset.Now - _lastRequestInstant;
                var remainingTime = interval - timePassedSinceLastRequest;
                if (remainingTime > TimeSpan.Zero)
                    await Task.Delay(remainingTime, cancellationToken);

                _lastRequestInstant = DateTimeOffset.Now;
            }
            finally
            {
                // Release the lock
                _requestRateSemaphore.Release();
            }
        }

        private async Task<JToken?> TryGetMusicBrainzTagsJsonAsync(
            string artist,
            string title,
            CancellationToken cancellationToken = default)
        {
            var url = Uri.EscapeUriString(
                "http://musicbrainz.org/ws/2/recording/?fmt=json&query=" +
                $"artist:\"{artist}\" AND recording:\"{title}\"");

            try
            {
                // 4 requests per second
                await MaintainRateLimitAsync(TimeSpan.FromSeconds(1.0 / 4), cancellationToken);

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return null;

                var raw = await response.Content.ReadAsStringAsync();

                return JToken.Parse(raw)["recordings"]?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private async Task<IPicture?> TryGetPictureAsync(
            IVideo video,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var thumbnail = video.Thumbnails.TryGetWithHighestResolution();
                if (thumbnail is null)
                    return null;

                using var response = await _httpClient.GetAsync(
                    thumbnail.Url,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken
                );

                if (!response.IsSuccessStatusCode)
                    return null;

                var data = await response.Content.ReadAsByteArrayAsync();

                return new Picture(new ReadOnlyByteVector(data));
            }
            catch
            {
                return null;
            }
        }

        private bool TryExtractArtistAndTitle(
            string videoTitle,
            out string? artist,
            out string? title)
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
            var split = videoTitle.Split(new[] {" - ", " ~ ", " — ", " – "}, StringSplitOptions.RemoveEmptyEntries);

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

        private async Task InjectVideoTagsAsync(
            IVideo video,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var picture = await TryGetPictureAsync(video, cancellationToken);

            var file = File.Create(filePath);

            var appleTag = file.GetTag(TagTypes.Apple) as AppleTag;
            appleTag?.SetDashBox("Channel", "Channel", video.Author.Title);

            file.Tag.Pictures = picture is not null
                ? new[] {picture}
                : Array.Empty<IPicture>();

            file.Save();
        }

        private async Task InjectAudioTagsAsync(
            IVideo video,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            if (!TryExtractArtistAndTitle(video.Title, out var artist, out var title))
                return;

            var tagsJson = await TryGetMusicBrainzTagsJsonAsync(artist!, title!, cancellationToken);

            var picture = await TryGetPictureAsync(video, cancellationToken);

            var resolvedArtist = tagsJson?["artist-credit"]?.FirstOrDefault()?["name"]?.Value<string>();
            var resolvedTitle = tagsJson?["title"]?.Value<string>();
            var resolvedAlbumName = tagsJson?["releases"]?.FirstOrDefault()?["title"]?.Value<string>();

            var file = File.Create(filePath);

            file.Tag.Performers = new[] {resolvedArtist ?? artist ?? ""};
            file.Tag.Title = resolvedTitle ?? title ?? "";
            file.Tag.Album = resolvedAlbumName ?? "";

            file.Tag.Pictures = picture is not null
                ? new[] {picture}
                : Array.Empty<IPicture>();

            file.Save();
        }

        public async Task InjectTagsAsync(
            IVideo video,
            string format,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            if (string.Equals(format, "mp4", StringComparison.OrdinalIgnoreCase))
            {
                await InjectVideoTagsAsync(video, filePath, cancellationToken);
            }
            else if (
                string.Equals(format, "mp3", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "ogg", StringComparison.OrdinalIgnoreCase))
            {
                await InjectAudioTagsAsync(video, filePath, cancellationToken);
            }

            // Other formats are not supported for tagging
        }
    }
}