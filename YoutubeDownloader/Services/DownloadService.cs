using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly SettingsService _settingsService;

        private readonly YoutubeClient _youtube = new YoutubeClient();
        private readonly IYoutubeConverter _youtubeConverter = new YoutubeConverter();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private int _concurrentDownloadCount;

        public DownloadService(SettingsService settingsService)
        {
            _settingsService = settingsService;

            // Increase maximum concurrent connections
            ServicePointManager.DefaultConnectionLimit = 20;
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

        public async Task DownloadVideoAsync(DownloadOption downloadOption, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            await EnsureThrottlingAsync(cancellationToken);

            try
            {
                await _youtubeConverter.DownloadAndProcessMediaStreamsAsync(downloadOption.StreamInfos,
                    filePath, downloadOption.Format, ConversionPreset.Medium,
                    progress, cancellationToken);
            }
            finally
            {
                Interlocked.Decrement(ref _concurrentDownloadCount);
            }
        }

        public async Task<IReadOnlyList<DownloadOption>> GetDownloadOptionsAsync(string videoId)
        {
            var options = new HashSet<DownloadOption>(DownloadOptionEqualityComparer.Instance);

            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

            // Best audio stream (muxed or audio-only)
            var bestAudioOnlyStreamInfo = streamManifest
                .GetAudio()
                .OrderByDescending(s => s.Container == Container.WebM)
                .ThenByDescending(s => s.Bitrate)
                .FirstOrDefault();

            // Video streams
            var videoStreams = streamManifest
                .GetVideo()
                .OrderByDescending(v => v.VideoQuality)
                .ThenByDescending(v => v.Framerate);

            foreach (var streamInfo in videoStreams)
            {
                var format = streamInfo.Container.Name;
                var label = streamInfo.VideoQualityLabel;

                // Muxed streams are standalone, video-only need to be merged with audio
                if (streamInfo is VideoOnlyStreamInfo && bestAudioOnlyStreamInfo != null)
                    options.Add(new DownloadOption(format, label, streamInfo, bestAudioOnlyStreamInfo));
                else
                    options.Add(new DownloadOption(format, label, streamInfo));
            }

            // Additional options
            if (bestAudioOnlyStreamInfo != null)
            {
                options.Add(new DownloadOption("mp3", "Audio", bestAudioOnlyStreamInfo));
                options.Add(new DownloadOption("ogg", "Audio", bestAudioOnlyStreamInfo));
            }

            return options.ToArray();
        }

        public async Task<DownloadOption> GetBestDownloadOptionAsync(string videoId, string format)
        {
            // Get all download options
            var downloadOptions = await GetDownloadOptionsAsync(videoId);

            // Get first download option for this format
            return downloadOptions.FirstOrDefault(o => o.Format == format);
        }
    }
}