using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliWrap.Builders;
using PowerKit;
using PowerKit.Extensions;
using YoutubeExplode.Converter.Utils.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter;

internal partial class Converter(VideoClient videoClient, FFmpeg ffmpeg, ConversionPreset preset)
{
    private async ValueTask ProcessAsync(
        string filePath,
        Container container,
        IReadOnlyList<StreamInput> streamInputs,
        IReadOnlyList<SubtitleInput> subtitleInputs,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var arguments = new ArgumentsBuilder();

        // Stream inputs
        foreach (var streamInput in streamInputs)
        {
            arguments.Add("-i").Add(streamInput.FilePath);
        }

        // Subtitle inputs
        foreach (var subtitleInput in subtitleInputs)
        {
            arguments
                // Fix invalid subtitle durations for each input
                // https://github.com/Tyrrrz/YoutubeExplode/issues/756
                .Add("-fix_sub_duration")
                .Add("-i")
                .Add(subtitleInput.FilePath);
        }

        // Explicitly specify that all inputs should be used, because by default
        // FFmpeg only picks one input per stream type (audio, video, subtitle).
        for (var i = 0; i < streamInputs.Count + subtitleInputs.Count; i++)
        {
            arguments.Add("-map").Add(i);
        }

        // Output format and encoding preset
        arguments.Add("-f").Add(container.Name).Add("-preset").Add(preset);

        // Avoid transcoding inputs that have the same container as the output
        {
            var lastAudioStreamIndex = 0;
            var lastVideoStreamIndex = 0;
            foreach (var streamInput in streamInputs)
            {
                // Note: a muxed stream input will map to two separate audio and video streams

                if (streamInput.Info is IAudioStreamInfo audioStreamInfo)
                {
                    if (audioStreamInfo.Container == container)
                    {
                        arguments.Add($"-c:a:{lastAudioStreamIndex}").Add("copy");
                    }

                    lastAudioStreamIndex++;
                }

                if (streamInput.Info is IVideoStreamInfo videoStreamInfo)
                {
                    if (videoStreamInfo.Container == container)
                    {
                        arguments.Add($"-c:v:{lastVideoStreamIndex}").Add("copy");
                    }

                    lastVideoStreamIndex++;
                }
            }
        }

        // MP4: explicitly specify the codec for subtitles, otherwise they don't get embedded.
        if (container == Container.Mp4 && subtitleInputs.Any())
        {
            arguments.Add("-c:s").Add("mov_text");
        }

        // MP3: force the audio streams to be re-encoded by explicitly specifying quality,
        // otherwise the output file metadata may end up being corrupted.
        // https://superuser.com/a/893044
        if (container == Container.Mp3)
        {
            // Have FFmpeg determine the best quality possible
            arguments.Add("-q:a").Add(0);
        }

        // Metadata for stream inputs
        {
            var lastAudioStreamIndex = 0;
            var lastVideoStreamIndex = 0;
            foreach (var streamInput in streamInputs)
            {
                // Note: a muxed stream input will map to two separate audio and video streams

                if (streamInput.Info is IAudioStreamInfo audioStreamInfo)
                {
                    // Contains language information
                    if (audioStreamInfo.AudioLanguage is not null)
                    {
                        // Language codes can be stored in any format, but most players expect
                        // three-letter codes, so we'll try to convert to that first.
                        var languageCode =
                            audioStreamInfo.AudioLanguage.Value.TryGetThreeLetterCode()
                            ?? audioStreamInfo.AudioLanguage.Value.Code;

                        arguments
                            .Add($"-metadata:s:a:{lastAudioStreamIndex}")
                            .Add($"language={languageCode}")
                            .Add($"-metadata:s:a:{lastAudioStreamIndex}")
                            .Add(
                                $"title={audioStreamInfo.AudioLanguage.Value.Name} | {audioStreamInfo.Bitrate}"
                            );
                    }
                    // Does not contain language information
                    else
                    {
                        arguments
                            .Add($"-metadata:s:a:{lastAudioStreamIndex}")
                            .Add($"title={audioStreamInfo.Bitrate}");
                    }

                    lastAudioStreamIndex++;
                }

                if (streamInput.Info is IVideoStreamInfo videoStreamInfo)
                {
                    arguments
                        .Add($"-metadata:s:v:{lastVideoStreamIndex}")
                        .Add(
                            $"title={videoStreamInfo.VideoQuality.Label} | {videoStreamInfo.Bitrate}"
                        );

                    lastVideoStreamIndex++;
                }
            }
        }

        // Metadata for subtitles
        foreach (var (i, subtitleInput) in subtitleInputs.Index())
        {
            // Language codes can be stored in any format, but most players expect
            // three-letter codes, so we'll try to convert to that first.
            var languageCode =
                subtitleInput.Info.Language.TryGetThreeLetterCode()
                ?? subtitleInput.Info.Language.Code;

            arguments
                .Add($"-metadata:s:s:{i}")
                .Add($"language={languageCode}")
                .Add($"-metadata:s:s:{i}")
                .Add($"title={subtitleInput.Info.Language.Name}");
        }

        // Enable progress reporting
        arguments
            // Info log level is required to extract total stream duration
            .Add("-loglevel")
            .Add("info")
            .Add("-stats");

        // Misc settings
        arguments
            .Add("-hide_banner")
            .Add("-threads")
            .Add(Environment.ProcessorCount)
            .Add("-nostdin")
            .Add("-y");

        // Output
        arguments.Add(filePath);

        await ffmpeg.ExecuteAsync(arguments.Build(), progress, cancellationToken);
    }

    private async ValueTask PopulateStreamInputsAsync(
        string baseFilePath,
        IReadOnlyList<IStreamInfo> streamInfos,
        ICollection<StreamInput> streamInputs,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var progressMuxer = progress?.Pipe(p => new ProgressMuxer(p));
        var progresses = streamInfos
            .Select(s => progressMuxer?.CreateInput(s.Size.MegaBytes))
            .ToArray();

        var lastIndex = 0;

        foreach (var (streamInfo, streamProgress) in streamInfos.Zip(progresses))
        {
            var streamInput = new StreamInput(
                streamInfo,
                $"{baseFilePath}.stream-{lastIndex++}.tmp"
            );

            streamInputs.Add(streamInput);

            await videoClient.Streams.DownloadAsync(
                streamInfo,
                streamInput.FilePath,
                streamProgress,
                cancellationToken
            );
        }

        progress?.Report(1);
    }

    private async ValueTask PopulateSubtitleInputsAsync(
        string baseFilePath,
        IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos,
        ICollection<SubtitleInput> subtitleInputs,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var progressMuxer = progress?.Pipe(p => new ProgressMuxer(p));
        var progresses = closedCaptionTrackInfos
            .Select(_ => progressMuxer?.CreateInput())
            .ToArray();

        var lastIndex = 0;

        foreach (var (trackInfo, trackProgress) in closedCaptionTrackInfos.Zip(progresses))
        {
            var subtitleInput = new SubtitleInput(
                trackInfo,
                $"{baseFilePath}.subtitles-{lastIndex++}.tmp"
            );

            subtitleInputs.Add(subtitleInput);

            await videoClient.ClosedCaptions.DownloadAsync(
                trackInfo,
                subtitleInput.FilePath,
                trackProgress,
                cancellationToken
            );
        }

        progress?.Report(1);
    }

    public async ValueTask ProcessAsync(
        string filePath,
        Container container,
        IReadOnlyList<IStreamInfo> streamInfos,
        IReadOnlyList<ClosedCaptionTrackInfo> closedCaptionTrackInfos,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!streamInfos.Any())
            throw new InvalidOperationException("No streams provided.");

        // Configure progress aggregation
        var progressMuxer = progress?.Pipe(p => new ProgressMuxer(p));
        var streamDownloadProgress = progressMuxer?.CreateInput();
        var subtitleDownloadProgress = progressMuxer?.CreateInput(0.01);
        var conversionProgress = progressMuxer?.CreateInput(
            0.05
                +
                // Increase weight for each stream that needs to be transcoded
                5 * streamInfos.Count(s => s.Container != container)
        );

        // Populate inputs
        var streamInputs = new List<StreamInput>(streamInfos.Count);
        var subtitleInputs = new List<SubtitleInput>(closedCaptionTrackInfos.Count);

        try
        {
            await PopulateStreamInputsAsync(
                filePath,
                streamInfos,
                streamInputs,
                streamDownloadProgress,
                cancellationToken
            );

            await PopulateSubtitleInputsAsync(
                filePath,
                closedCaptionTrackInfos,
                subtitleInputs,
                subtitleDownloadProgress,
                cancellationToken
            );

            await ProcessAsync(
                filePath,
                container,
                streamInputs,
                subtitleInputs,
                conversionProgress,
                cancellationToken
            );
        }
        finally
        {
            foreach (var inputStream in streamInputs)
                inputStream.Dispose();

            foreach (var inputClosedCaptionTrack in subtitleInputs)
                inputClosedCaptionTrack.Dispose();
        }
    }
}

internal partial class Converter
{
    private class StreamInput(IStreamInfo info, string filePath) : IDisposable
    {
        public IStreamInfo Info { get; } = info;

        public string FilePath { get; } = filePath;

        public void Dispose()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch
            {
                // Ignore
            }
        }
    }

    private class SubtitleInput(ClosedCaptionTrackInfo info, string filePath) : IDisposable
    {
        public ClosedCaptionTrackInfo Info { get; } = info;

        public string FilePath { get; } = filePath;

        public void Dispose()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch
            {
                // Ignore
            }
        }
    }
}
