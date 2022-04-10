using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public record QueryResult(
    QueryResultKind Kind,
    string Title,
    IReadOnlyList<IVideo> Videos
);