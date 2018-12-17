using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter;

namespace YoutubeDownloader.Services
{
    public class DownloadService
    {
        private readonly SettingsService _settingsService;

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

        public async Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            // Ensure throttling and increment concurrent download count
            await EnsureThrottlingAsync(cancellationToken);

            try
            {
                // Download the video
                await _youtubeConverter.DownloadVideoAsync(videoId, filePath, progress, cancellationToken);
            }
            finally
            {
                // Decrement concurrent download count
                Interlocked.Decrement(ref _concurrentDownloadCount);
            }
        }
    }
}