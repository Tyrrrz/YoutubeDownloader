using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeDownloader.Services
{
    public class DownloadService
    {
        private readonly SettingsService _settingsService;

        private readonly IYoutubeClient _youtubeClient = new YoutubeClient();
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
                // Spin-wait until other downloads finish so that the number of concurrent downloads doesn't exceed the maximum
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

        public async Task<IReadOnlyList<DownloadOption>> GetVideoDownloadOptionsAsync(string videoId)
        {
            var result = new List<DownloadOption>();

            // Get media stream info set
            var mediaStreamInfoSet = await _youtubeClient.GetVideoMediaStreamInfosAsync(videoId);

            // Sort and filter video streams
            var videoStreamInfos = mediaStreamInfoSet.Video
                .OrderByDescending(s => s.VideoQuality)
                .ThenByDescending(s => s.Framerate)
                .ToArray();

            // Add video download options
            foreach (var videoStreamInfo in videoStreamInfos)
            {
                // Get format
                var format = videoStreamInfo.Container.GetFileExtension();

                // Get best audio stream, preferably with the same container
                var audioStreamInfo = mediaStreamInfoSet.Audio
                    .OrderByDescending(s => s.Container == videoStreamInfo.Container)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                // Add to list
                result.Add(new DownloadOption(format, audioStreamInfo, videoStreamInfo));
            }

            // Add audio-only download options
            {
                // Get best audio stream, preferably with webm container
                var audioStreamInfo = mediaStreamInfoSet.Audio
                    .OrderByDescending(s => s.Container == Container.WebM)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                // Add to list
                result.Add(new DownloadOption("mp3", audioStreamInfo));
                result.Add(new DownloadOption("ogg", audioStreamInfo));
            }

            return result;
        }

        public async Task DownloadVideoAsync(string videoId, string filePath, DownloadOption downloadOption,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Ensure throttling and increment concurrent download count
            await EnsureThrottlingAsync(cancellationToken);

            try
            {
                // Download the video
                await _youtubeConverter.DownloadAndProcessMediaStreamsAsync(downloadOption.MediaStreamInfos,
                    filePath, downloadOption.Format,
                    progress, cancellationToken);
            }
            finally
            {
                // Decrement concurrent download count
                Interlocked.Decrement(ref _concurrentDownloadCount);
            }
        }

        public async Task DownloadVideoAsync(string videoId, string filePath, string format,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Get download options
            var downloadOptions = await GetVideoDownloadOptionsAsync(videoId);

            // Get first download option for this format
            var downloadOption = downloadOptions.FirstOrDefault(o => o.Format == format);

            // Download the video
            await DownloadVideoAsync(videoId, filePath, downloadOption, progress, cancellationToken);
        }
    }
}