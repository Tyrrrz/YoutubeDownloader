using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter;

namespace YoutubeDownloader.Services
{
    public class DownloadService
    {
        private readonly IYoutubeConverter _converter = new YoutubeConverter();

        public Task DownloadVideoAsync(string videoId, string filePath,
            IProgress<double> progress, CancellationToken cancellationToken) =>
            _converter.DownloadVideoAsync(videoId, filePath, progress, cancellationToken);
    }
}