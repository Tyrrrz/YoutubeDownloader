using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public record QueryResult(
    QueryKind Kind,
    string Label,
    IReadOnlyList<IVideo> Videos
);