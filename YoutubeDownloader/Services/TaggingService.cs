using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
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

        private async Task<IPicture> TryGetPictureAsync(
            IVideo video,
            CancellationToken cancellationToken = default,
            string url = null)
        {
            try
            {
                var thumbnail = video.Thumbnails.TryGetWithHighestResolution();
                if (thumbnail is null)
                    return null;

                using var response = await _httpClient.GetAsync(
                    url ?? thumbnail.Url,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken
                );

                if (!response.IsSuccessStatusCode)
                    return null;
                var data = await response.Content.ReadAsByteArrayAsync();
                var cover = new TagLib.Id3v2.AttachmentFrame
                {
                    Type = PictureType.FrontCover,
                    Description = "Cover",
                    MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                    Data = new ByteVector(data),
                    TextEncoding = StringType.Latin1
                };
                var pic = new IPicture[] { cover };

                return pic.FirstOrDefault();
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

        private async Task<string> InjectVideoTagsAsync(
            IVideo video,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var picture = await TryGetPictureAsync(video, cancellationToken);

            var file = File.Create(filePath);

            var appleTag = file.GetTag(TagTypes.Apple) as AppleTag;
            appleTag?.SetDashBox("Channel", "Channel", video.Author.Title);

            var picturestoadd = picture is not null
                ? new[] { picture }
                : Array.Empty<IPicture>();

            file.Tag.Pictures = picturestoadd;

            file.Save();

            return filePath;
        }

        private async Task<string> InjectAudioTagsAsync(
            IVideo video,
            string filePath,
            string format,
            bool AutoRename,
            List<string> shazamapikeys,
            List<string> vagalumeapikeys,
            CancellationToken cancellationToken = default)
        {
            // 4 requests per second
            await MaintainRateLimitAsync(TimeSpan.FromSeconds(1.0 / 4), cancellationToken);

            FileInfo info = new FileInfo(filePath);
            string filetitle = info.Name;

            if (!ShazamMusicInfo.TryExtractArtistAndTitle(filetitle.Replace(info.Extension, "")
                , shazamapikeys
                , vagalumeapikeys
                , out string artist
                , out string title
                , out string picturelink
                , out string track))
                return filePath;

            bool somethingfound = false;
            var normalizedartist = RemoveDiacritics(artist);
            var normalizedtitle = RemoveDiacritics(title);

            foreach (var item in filetitle.Replace(info.Extension, "").Split(" "))
            {
                if (string.IsNullOrEmpty(item) || item.Length <= 3)
                    continue;

                var normalizeditem = RemoveDiacritics(item);

                if (string.Equals(normalizeditem, normalizedartist, StringComparison.OrdinalIgnoreCase))
                {
                    somethingfound = true;
                    break;
                }
                if (string.Equals(normalizeditem, normalizedtitle, StringComparison.OrdinalIgnoreCase))
                {
                    somethingfound = true;
                    break;
                }
            }

            if (!somethingfound
                && (!filetitle.Contains(normalizedartist) || normalizedartist.Length <= 3)
                && (!filetitle.Contains(normalizedtitle) || normalizedtitle.Length <= 3)
                )
            {
                var _picture = await TryGetPictureAsync(video, cancellationToken);
                var _file = File.Create(filePath);
                IPicture[] _pictFrames = new IPicture[1];
                _pictFrames[0] = _picture;
                var _picturestoadd = _pictFrames;

                _file.Tag.Pictures = _picturestoadd;

                _file.Save();
                _file.Dispose();
                return filePath;
            }

            var tagsJson = await TryGetMusicBrainzTagsJsonAsync(artist!, title!, cancellationToken);

            var picture = await TryGetPictureAsync(video, cancellationToken, picturelink);

            var resolvedArtist = tagsJson?["artist-credit"]?.FirstOrDefault()?["name"]?.Value<string>();
            var resolvedTitle = tagsJson?["title"]?.Value<string>();
            var resolvedAlbumName = track ?? tagsJson?["releases"]?.FirstOrDefault()?["title"]?.Value<string>();

            var file = File.Create(filePath);

            file.Tag.Performers = new[] { artist ?? resolvedArtist ?? "" };
            file.Tag.Title = title ?? resolvedTitle ?? "";
            file.Tag.Album = resolvedAlbumName ?? "";

            //BPMDetector only in 32bit environment
            if (!Environment.Is64BitProcess)
            {
                double bitrate = 44100;
                if (string.Equals(format, "wav", StringComparison.OrdinalIgnoreCase))
                    using (var reader = new WaveFileReader(filePath))
                    {
                        bitrate = reader.WaveFormat.AverageBytesPerSecond * 8;
                        bitrate = bitrate / 1000;
                    }
                if (string.Equals(format, "mp3", StringComparison.OrdinalIgnoreCase))
                    using (var reader = new Mp3FileReader(filePath))
                    {
                        bitrate = reader.WaveFormat.AverageBytesPerSecond * 8;
                        bitrate = bitrate / 1000;
                    }

                var bpmdetector = new BPMDetector(filePath, Convert.ToInt32(bitrate));
                var bpm = bpmdetector.getBPM();

                file.Tag.BeatsPerMinute = Convert.ToUInt32(bpm);
            }

            IPicture[] pictFrames = new IPicture[1];
            pictFrames[0] = picture;
            var picturestoadd = pictFrames;

            file.Tag.Pictures = picturestoadd;

            file.Save();
            file.Dispose();

            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var newfilename = $"{artist ?? resolvedArtist} - {title ?? resolvedTitle}";
            foreach (char c in invalid)
            {
                newfilename = newfilename.Replace(c.ToString(), "");
            }

            string newfilepath = Path.Combine(info.Directory.FullName, $"{newfilename}{info.Extension}");
            if (!System.IO.File.Exists(newfilepath))
            {
                info.MoveTo(Path.Combine(info.Directory.FullName, $"{newfilename}{info.Extension}"), true);
                return newfilepath;
            }
            return filePath;
        }

        public async Task<string> InjectTagsAsync(
            IVideo video,
            string format,
            string filePath,
            bool AutoRename,
            List<string> shazamapikeys,
            List<string> vagalumeapikeys,
            CancellationToken cancellationToken = default)
        {
            if (string.Equals(format, "mp4", StringComparison.OrdinalIgnoreCase))
            {
                return await InjectVideoTagsAsync(video, filePath, cancellationToken);
            }
            else if (
                string.Equals(format, "mp3", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "wav", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "ogg", StringComparison.OrdinalIgnoreCase))
            {
                return await InjectAudioTagsAsync(video, filePath, format, AutoRename, shazamapikeys, vagalumeapikeys, cancellationToken);
            }

            return filePath;
            // Other formats are not supported for tagging
        }

        static string RemoveDiacritics(string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}