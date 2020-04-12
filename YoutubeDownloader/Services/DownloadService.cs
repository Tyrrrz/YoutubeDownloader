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
            // Gain lock
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                // Wait until other downloads finish so that the number of concurrent downloads doesn't exceed the maximum
                while (_concurrentDownloadCount >= _settingsService.MaxConcurrentDownloadCount)
                    await Task.Delay(350, cancellationToken);

                // Increment concurrent download count
                Interlocked.Increment(ref _concurrentDownloadCount);
            }
            finally
            {
                // Release the lock
                _semaphore.Release();
            }
        }

        public async Task DownloadVideoAsync(DownloadOption downloadOption, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Ensure throttling and increment concurrent download count
            await EnsureThrottlingAsync(cancellationToken);

            try
            {
                // Download the video
                await _youtubeConverter.DownloadAndProcessMediaStreamsAsync(downloadOption.StreamInfos,
                    filePath, downloadOption.Format, ConversionPreset.Medium,
                    progress, cancellationToken);
            }
            finally
            {
                // Decrement concurrent download count
                Interlocked.Decrement(ref _concurrentDownloadCount);
            }
        }

        public async Task<IReadOnlyList<DownloadOption>> GetDownloadOptionsAsync(string videoId)
        {
            var result = new List<DownloadOption>();

            // Get media stream info set
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

            // Prefer adaptive streams if possible
            if (streamManifest.GetAudioOnly().Any() && streamManifest.GetVideoOnly().Any())
            {
                // Sort video streams
                var videoStreamInfos = streamManifest
                    .GetVideoOnly()
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => s.Framerate)
                    .ToArray();

                // Add video download options
                foreach (var videoStreamInfo in videoStreamInfos)
                {
                    // Get format
                    var format = videoStreamInfo.Container.Name;

                    // Get best audio stream, preferably with the same container
                    var audioStreamInfo = streamManifest
                        .GetAudioOnly()
                        .OrderByDescending(s => s.Container == videoStreamInfo.Container)
                        .ThenByDescending(s => s.Bitrate)
                        .First();

                    // Add to list
                    result.Add(new DownloadOption(format, audioStreamInfo, videoStreamInfo));
                }

                // Add audio-only download options
                {
                    // Get best audio stream, preferably with webm container
                    var audioStreamInfo = streamManifest
                        .GetAudioOnly()
                        .OrderByDescending(s => s.Container == Container.WebM)
                        .ThenByDescending(s => s.Bitrate)
                        .First();

                    // Add to list
                    result.Add(new DownloadOption("mp3", audioStreamInfo));
                    result.Add(new DownloadOption("ogg", audioStreamInfo));
                }
            }
            // Fallback to muxed streams
            else if (streamManifest.GetMuxed().Any())
            {
                // Sort muxed streams
                var muxedStreamInfos = streamManifest
                    .GetMuxed()
                    .OrderByDescending(s => s.VideoQuality)
                    .ToArray();

                // Add video download options
                foreach (var muxedStreamInfo in muxedStreamInfos)
                {
                    // Get format
                    var format = muxedStreamInfo.Container.Name;

                    // Add to list
                    result.Add(new DownloadOption(format, muxedStreamInfo));
                }

                // Add audio-only download options
                {
                    // Use best muxed stream as the audio stream, preferably with webm container
                    var bestMuxedStreamInfo = muxedStreamInfos
                        .OrderByDescending(s => s.Container == Container.WebM)
                        .First();

                    // Add to list
                    result.Add(new DownloadOption("mp3", bestMuxedStreamInfo));
                    result.Add(new DownloadOption("ogg", bestMuxedStreamInfo));
                }
            }

            return result;
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