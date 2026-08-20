using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter;

/// <summary>
/// Extensions for <see cref="VideoClient" /> that provide interface for downloading videos through FFmpeg.
/// </summary>
public static class ConversionExtensions
{
    /// <inheritdoc cref="ConversionExtensions" />
    extension(Container container)
    {
        /// <summary>
        /// Checks whether the container is a known audio-only container.
        /// </summary>
        [Obsolete("Use the Container.IsAudioOnly property instead."), ExcludeFromCodeCoverage]
        public bool IsAudioOnly() => container.IsAudioOnly;
    }

    /// <inheritdoc cref="ConversionExtensions" />
    extension(VideoClient videoClient)
    {
        private async IAsyncEnumerable<IStreamInfo> GetOptimalStreamInfosAsync(
            VideoId videoId,
            Container container,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            var streamManifest = await videoClient.Streams.GetManifestAsync(
                videoId,
                cancellationToken
            );

            if (
                streamManifest.GetAudioOnlyStreams().Any()
                && streamManifest.GetVideoOnlyStreams().Any()
            )
            {
                // Include audio stream
                // Priority: transcoding -> bitrate
                yield return streamManifest
                    .GetAudioOnlyStreams()
                    .OrderByDescending(s => s.Container == container)
                    .ThenByDescending(s => s.Bitrate)
                    .First();

                // Include video stream
                if (!container.IsAudioOnly)
                {
                    // Priority: video quality -> transcoding
                    yield return streamManifest
                        .GetVideoOnlyStreams()
                        .OrderByDescending(s => s.VideoQuality)
                        .ThenByDescending(s => s.Container == container)
                        .First();
                }
            }
            // Use single muxed stream if adaptive streams are not available
            else
            {
                // Priority: video quality -> transcoding
                yield return streamManifest
                    .GetMuxedStreams()
                    .OrderByDescending(s => s.VideoQuality)
                    .ThenByDescending(s => s.Container == container)
                    .First();
            }
        }

        /// <summary>
        /// Downloads the specified media streams and closed captions and processes them into a single file.
        /// </summary>
        public async ValueTask DownloadAsync(
            IReadOnlyList<IStreamInfo> streamInfos,
            IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos,
            ConversionRequest request,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            var ffmpeg = new FFmpeg(request.FFmpegCliFilePath, request.EnvironmentVariables);
            var converter = new Converter(videoClient, ffmpeg, request.Preset);

            await converter.ProcessAsync(
                request.OutputFilePath,
                request.Container,
                streamInfos,
                closedCaptionTrackInfos,
                progress,
                cancellationToken
            );
        }

        /// <summary>
        /// Downloads the specified media streams and processes them into a single file.
        /// </summary>
        public async ValueTask DownloadAsync(
            IReadOnlyList<IStreamInfo> streamInfos,
            ConversionRequest request,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default
        ) => await videoClient.DownloadAsync(streamInfos, [], request, progress, cancellationToken);

        /// <summary>
        /// Resolves the most optimal media streams for the specified video, downloads them,
        /// and processes into a single file.
        /// </summary>
        public async ValueTask DownloadAsync(
            VideoId videoId,
            ConversionRequest request,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default
        ) =>
            await videoClient.DownloadAsync(
                await videoClient.GetOptimalStreamInfosAsync(
                    videoId,
                    request.Container,
                    cancellationToken
                ),
                request,
                progress,
                cancellationToken
            );

        /// <summary>
        /// Resolves the most optimal media streams for the specified video, downloads them,
        /// and processes into a single file.
        /// </summary>
        /// <remarks>
        /// Output container is inferred from the file extension, unless explicitly specified.
        /// </remarks>
        public async ValueTask DownloadAsync(
            VideoId videoId,
            string outputFilePath,
            Action<ConversionRequestBuilder> configure,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            var requestBuilder = new ConversionRequestBuilder(outputFilePath);
            configure(requestBuilder);
            var request = requestBuilder.Build();

            await videoClient.DownloadAsync(videoId, request, progress, cancellationToken);
        }

        /// <summary>
        /// Resolves the most optimal media streams for the specified video,
        /// downloads them, and processes into a single file.
        /// </summary>
        /// <remarks>
        /// Output container is inferred from the file extension.
        /// If none is specified, mp4 is chosen by default.
        /// </remarks>
        public async ValueTask DownloadAsync(
            VideoId videoId,
            string outputFilePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default
        ) =>
            await videoClient.DownloadAsync(
                videoId,
                outputFilePath,
                _ => { },
                progress,
                cancellationToken
            );
    }
}
