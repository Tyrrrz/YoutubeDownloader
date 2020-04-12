using System.Collections.Generic;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Models
{
    public class DownloadOption
    {
        public string Format { get; }

        public string Label { get; }

        public IReadOnlyList<IStreamInfo> StreamInfos { get; }

        public DownloadOption(string format, string label, IReadOnlyList<IStreamInfo> streamInfos)
        {
            Format = format;
            Label = label;
            StreamInfos = streamInfos;
        }

        public DownloadOption(string format, AudioOnlyStreamInfo audioOnlyStreamInfo)
            : this(format, "Audio", new[] {audioOnlyStreamInfo})
        {
        }

        public DownloadOption(string format, AudioOnlyStreamInfo audioOnlyStreamInfo, VideoOnlyStreamInfo videoOnlyStreamInfo)
            : this(format, videoOnlyStreamInfo.VideoQualityLabel, new IStreamInfo[] {audioOnlyStreamInfo, videoOnlyStreamInfo})
        {
        }

        public DownloadOption(string format, MuxedStreamInfo muxedStreamInfo)
            : this(format, muxedStreamInfo.VideoQualityLabel, new[] {muxedStreamInfo})
        {
        }

        public override string ToString() => $"{Label} / {Format}";
    }
}