using System.Collections.Generic;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Models
{
    public class SubtitleOption
    {
        public Language Language { get; set; }

        public IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTrackInfos { get; }

        public SubtitleOption(Language language, IReadOnlyList<ClosedCaptionTrackInfo> trackInfos)
        {
            Language = language;
            ClosedCaptionTrackInfos = trackInfos;
        }

        public SubtitleOption(Language language, params ClosedCaptionTrackInfo[] trackInfos)
            : this(language, (IReadOnlyList<ClosedCaptionTrackInfo>)trackInfos)
        {
        }

        public override string ToString() => $"{Language.Name}" + (!string.IsNullOrWhiteSpace(Language.Code) ? $" ({Language.Code})" : string.Empty);
    }
}
