using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public record YoutubeQueryResult(
    YoutubeQueryKind Kind,
    string Label,
    IReadOnlyList<IVideo> Videos
);