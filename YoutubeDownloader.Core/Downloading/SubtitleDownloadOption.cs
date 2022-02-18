using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Core.Downloading;

public partial record SubtitleDownloadOption(ClosedCaptionTrackInfo TrackInfo)
{
    public string Label => TrackInfo.Language.Name; // already includes "auto-generated"
}

public partial record SubtitleDownloadOption
{
    internal static IReadOnlyList<SubtitleDownloadOption> ResolveAll(ClosedCaptionManifest manifest) => manifest.Tracks
        .Select(t => new SubtitleDownloadOption(t))
        .ToArray();
}