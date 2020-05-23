using System;
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

    public class DownloadOptionEqualityComparer : IEqualityComparer<DownloadOption>
    {
        public static DownloadOptionEqualityComparer Instance { get; } = new DownloadOptionEqualityComparer();

        public bool Equals(DownloadOption x, DownloadOption y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x.Format, y.Format) &&
            StringComparer.OrdinalIgnoreCase.Equals(x.Label, y.Label);

        public int GetHashCode(DownloadOption obj) => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Format),
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Label)
        );
    }
}