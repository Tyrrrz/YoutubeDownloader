using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.ClosedCaptions;
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

        public async Task DownloadSubtitleAsync(SubtitleOption subtitleOption, string filePath)
        {
            await _youtube.Videos.ClosedCaptions.DownloadAsync(subtitleOption.ClosedCaptionTrackInfos.FirstOrDefault(), 
                $"{Path.GetDirectoryName(filePath)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(filePath)}.srt");
        }

        public async Task<IReadOnlyList<DownloadOption>> GetDownloadOptionsAsync(string videoId)
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

            // Using a set ensures only one download option per format/quality is provided
            var options = new HashSet<DownloadOption>(DownloadOptionEqualityComparer.Instance);

            // Video+audio options
            var videoStreams = streamManifest
                .GetVideo()
                .Where(v => !_settingsService.ExcludedContainerFormats.Contains(v.Container.Name))
                .OrderByDescending(v => v.VideoQuality)
                .ThenByDescending(v => v.Framerate);

            foreach (var streamInfo in videoStreams)
            {
                var format = streamInfo.Container.Name;
                var label = streamInfo.VideoQualityLabel;

                // Muxed streams are standalone
                if (streamInfo is MuxedStreamInfo)
                {
                    options.Add(new DownloadOption(format, label, streamInfo));
                    continue;
                }

                // Get audio with matching format, if possible
                var audioStreamInfo = streamManifest
                    .GetAudio()
                    .OrderByDescending(s => s.Container == streamInfo.Container)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo != null)
                {
                    options.Add(new DownloadOption(format, label, streamInfo, audioStreamInfo));
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
                if (!_settingsService.ExcludedContainerFormats.Contains("mp3"))
                    options.Add(new DownloadOption("mp3", "Audio", bestAudioOnlyStreamInfo));

                if (!_settingsService.ExcludedContainerFormats.Contains("ogg"))
                    options.Add(new DownloadOption("ogg", "Audio", bestAudioOnlyStreamInfo));
            }

            return options.ToArray();
        }

        public async Task<IReadOnlyList<SubtitleOption>> GetSubtitleOptionsAsync(string videoId)
        {
            var closedCaptionManifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            var options = new HashSet<SubtitleOption>();

            options.Add(new SubtitleOption(new Language(string.Empty, "No subtitle")));

            foreach (var closedCaptionTrackInfo in closedCaptionManifest.Tracks)
            {
                options.Add(new SubtitleOption(closedCaptionTrackInfo.Language, closedCaptionTrackInfo));
            }

            return options.ToArray();
        }

        public async Task<DownloadOption> GetBestDownloadOptionAsync(string videoId, string format, DownloadQuality quality)
        {
            // Get all download options
            var downloadOptions = await GetDownloadOptionsAsync(videoId);

            // Go from worst quality download option up to highest requested
            var bestOption = downloadOptions
                .Where(o => o.Format == format)
                .Reverse()
                .TakeWhile(o => IsAcceptableQuality(o.StreamInfos, quality))                
                .LastOrDefault();

            if (bestOption != null)
                return bestOption;

            // Get first download option for this format only
            return downloadOptions.FirstOrDefault(o => o.Format == format);
        }

        private bool IsAcceptableQuality(IReadOnlyList<IStreamInfo> streamInfos, DownloadQuality quality)
        {
            var videoStreamInfo = streamInfos.Where(s => s is IVideoStreamInfo).Cast<IVideoStreamInfo>().FirstOrDefault();
            if (videoStreamInfo == null)
                return false;

            switch (quality)
            {
                case DownloadQuality.Minimum:
                    return videoStreamInfo.VideoQuality <= VideoQuality.Low144;
                case DownloadQuality.Low:
                    return videoStreamInfo.VideoQuality <= VideoQuality.Medium480;
                case DownloadQuality.Medium:
                    return videoStreamInfo.VideoQuality <= VideoQuality.High720;
                case DownloadQuality.High:
                    return videoStreamInfo.VideoQuality <= VideoQuality.High1080;
                case DownloadQuality.Maximum:
                default:
                    return true;
            }
        }
    }
}