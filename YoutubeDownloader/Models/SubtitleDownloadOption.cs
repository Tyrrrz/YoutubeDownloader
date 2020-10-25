using System;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Models
{
    public partial class SubtitleDownloadOption
    {
        public ClosedCaptionTrackInfo TrackInfo { get; }

        public SubtitleDownloadOption(ClosedCaptionTrackInfo trackInfo)
        {
            TrackInfo = trackInfo;
        }

        public override string ToString() => TrackInfo.Language.ToString();
    }

    public partial class SubtitleDownloadOption : IEquatable<SubtitleDownloadOption>
    {
        public bool Equals(SubtitleDownloadOption? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return StringComparer.Ordinal.Equals(TrackInfo.Url, other.TrackInfo.Url);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((SubtitleDownloadOption) obj);
        }

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(TrackInfo.Url);
    }
}