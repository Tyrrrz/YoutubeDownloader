using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Models
{
    public partial class VideoDownloadOption
    {
        public string Format { get; }

        public string Label { get; }

        public IReadOnlyList<IStreamInfo> StreamInfos { get; }

        public VideoQuality? Quality =>
            StreamInfos.OfType<IVideoStreamInfo>()
                .Select(s => s.VideoQuality)
                .OrderByDescending(q => q)
                .FirstOrDefault();

        public VideoDownloadOption(
            string format,
            string label,
            IReadOnlyList<IStreamInfo> streamInfos)
        {
            Format = format;
            Label = label;
            StreamInfos = streamInfos;
        }

        public VideoDownloadOption(
            string format,
            string label,
            params IStreamInfo[] streamInfos)
            : this(format, label, (IReadOnlyList<IStreamInfo>) streamInfos) {}

        public override string ToString() => $"{Label} / {Format}";
    }

    public partial class VideoDownloadOption : IEquatable<VideoDownloadOption>
    {
        public bool Equals(VideoDownloadOption? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                StringComparer.OrdinalIgnoreCase.Equals(Format, other.Format) &&
                StringComparer.OrdinalIgnoreCase.Equals(Label, other.Label);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((VideoDownloadOption) obj);
        }

        public override int GetHashCode() => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Format),
            StringComparer.OrdinalIgnoreCase.GetHashCode(Label)
        );
    }
}