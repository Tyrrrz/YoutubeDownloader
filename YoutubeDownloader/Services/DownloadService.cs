using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter;

namespace YoutubeDownloader.Services
{
    public class DownloadService
    {
        private readonly IYoutubeConverter _youtubeConverter = new YoutubeConverter();

        public Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken) =>
            _youtubeConverter.DownloadVideoAsync(videoId, filePath, progress, cancellationToken);
    }
}