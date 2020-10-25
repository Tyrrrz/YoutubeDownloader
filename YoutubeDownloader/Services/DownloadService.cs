using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Services
{
    public class DownloadService
    {
        private readonly YoutubeClient _youtube = new YoutubeClient();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly SettingsService _settingsService;

        private int _concurrentDownloadCount;

        public DownloadService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private async Task EnsureThrottlingAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                // Wait until other downloads finish so that the number of concurrent downloads doesn't exceed the maximum
                while (_concurrentDownloadCount >= _settingsService.MaxConcurrentDownloadCount)
                    await Task.Delay(350, cancellationToken);

                Interlocked.Increment(ref _concurrentDownloadCount);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DownloadAsync(
            VideoDownloadOption videoDownloadOption,
            SubtitleDownloadOption? subtitleDownloadOption,
            string filePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureThrottlingAsync(cancellationToken);

            try
            {
                var conversion = new ConversionRequestBuilder(filePath)
                    .SetFormat(videoDownloadOption.Format)
                    .SetPreset(ConversionPreset.Medium)
                    .Build();

                await _youtube.Videos.DownloadAsync(
                    videoDownloadOption.StreamInfos,
                    conversion,
                    progress,
                    cancellationToken
                );

                if (subtitleDownloadOption != null)
                {
                    var subtitleFilePath = Path.ChangeExtension(filePath, "srt");

                    // Not passing progress because it's insignificant and would require splitting
                    await _youtube.Videos.ClosedCaptions.DownloadAsync(
                        subtitleDownloadOption.TrackInfo,
                        subtitleFilePath,
                        null,
                        cancellationToken
                    );
                }
            }
            finally
            {
                Interlocked.Decrement(ref _concurrentDownloadCount);
            }
        }

        public async Task<IReadOnlyList<VideoDownloadOption>> GetVideoDownloadOptionsAsync(string videoId)
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

            // Using a set ensures only one download option per format/quality is provided
            var options = new HashSet<VideoDownloadOption>();

            // Video+audio options
            var videoStreams = streamManifest
                .GetVideo()
                .OrderByDescending(v => v.VideoQuality)
                .ThenByDescending(v => v.Framerate);

            foreach (var streamInfo in videoStreams)
            {
                var format = streamInfo.Container.Name;
                var label = streamInfo.VideoQualityLabel;

                // Muxed streams are standalone
                if (streamInfo is MuxedStreamInfo)
                {
                    options.Add(new VideoDownloadOption(format, label, streamInfo));
                    continue;
                }

                // Get audio with matching format, if possible
                var audioStreamInfo =
                    (IStreamInfo?)
                    streamManifest
                        .GetAudioOnly()
                        .OrderByDescending(s => s.Container == streamInfo.Container)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault() ??
                    streamManifest
                        .GetMuxed()
                        .OrderByDescending(s => s.Container == streamInfo.Container)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();

                if (audioStreamInfo != null)
                {
                    options.Add(new VideoDownloadOption(format, label, streamInfo, audioStreamInfo));
                }
            }

            // Audio-only options
            var bestAudioOnlyStreamInfo = streamManifest
                .GetAudio()
                .OrderByDescending(s => s.Container == Container.WebM)
                .ThenByDescending(s => s.Bitrate)
                .FirstOrDefault();

            if (bestAudioOnlyStreamInfo != null)
            {
                options.Add(new VideoDownloadOption("mp3", "Audio", bestAudioOnlyStreamInfo));
                options.Add(new VideoDownloadOption("ogg", "Audio", bestAudioOnlyStreamInfo));
            }

            // Drop excluded formats
            if (_settingsService.ExcludedContainerFormats != null)
            {
                options.RemoveWhere(o =>
                    _settingsService.ExcludedContainerFormats.Contains(o.Format, StringComparer.OrdinalIgnoreCase)
                );
            }

            return options.ToArray();
        }

        public async Task<IReadOnlyList<SubtitleDownloadOption>> GetSubtitleDownloadOptionsAsync(string videoId)
        {
            var closedCaptionManifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            return closedCaptionManifest.Tracks
                .Select(t => new SubtitleDownloadOption(t))
                .ToArray();
        }

        public async Task<VideoDownloadOption?> TryGetBestVideoDownloadOptionAsync(
            string videoId,
            string format,
            VideoQualityPreference qualityPreference)
        {
            var options = await GetVideoDownloadOptionsAsync(videoId);

            // TODO: generalize supported formats
            // Short-circuit for audio-only formats
            if (string.Equals(format, "mp3", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "ogg", StringComparison.OrdinalIgnoreCase))
            {
                return options.FirstOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase));
            }

            var orderedOptions = options
                .OrderBy(o => o.Quality)
                .ThenBy(o => o.Framerate)
                .ToArray();

            var preferredOption = qualityPreference switch
            {
                VideoQualityPreference.Maximum => orderedOptions
                    .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

                VideoQualityPreference.High => orderedOptions
                    .Where(o => o.Quality <= VideoQuality.High1080)
                    .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

                VideoQualityPreference.Medium => orderedOptions
                    .Where(o => o.Quality <= VideoQuality.High720)
                    .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

                VideoQualityPreference.Low => orderedOptions
                    .Where(o => o.Quality <= VideoQuality.Medium480)
                    .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

                VideoQualityPreference.Minimum => orderedOptions
                    .FirstOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

                _ => throw new ArgumentOutOfRangeException(nameof(qualityPreference))
            };

            return
                preferredOption ??
                orderedOptions.FirstOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase));
        }
    }
}