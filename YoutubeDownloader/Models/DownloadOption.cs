using System.Collections.Generic;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeDownloader.Models
{
    public class DownloadOption
    {
        public string Format { get; }

        public string Label { get; }

        public IReadOnlyList<MediaStreamInfo> MediaStreamInfos { get; }

        public DownloadOption(string format, string label, IReadOnlyList<MediaStreamInfo> mediaStreamInfos)
        {
            Format = format;
            Label = label;
            MediaStreamInfos = mediaStreamInfos;
        }

        public DownloadOption(string format, AudioStreamInfo audioStreamInfo)
            : this(format, "Audio", new[] {audioStreamInfo})
        {
        }

        public DownloadOption(string format, AudioStreamInfo audioStreamInfo, VideoStreamInfo videoStreamInfo)
            : this(format, videoStreamInfo.VideoQualityLabel, new MediaStreamInfo[] {audioStreamInfo, videoStreamInfo})
        {
        }

        public override string ToString() => $"{Label} / {Format}";
    }
}