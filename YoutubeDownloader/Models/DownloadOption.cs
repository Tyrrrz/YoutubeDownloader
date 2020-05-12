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

        public DownloadOption(string format, string label, params IStreamInfo[] streamInfos)
            : this(format, label, (IReadOnlyList<IStreamInfo>) streamInfos)
        {
        }

        public override string ToString() => $"{Label} / {Format}";
    }
}