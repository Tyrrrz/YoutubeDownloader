namespace YoutubeDownloader.Core.Resolving;

public record QueryResult(
    QueryResultKind Kind,
    string Title,
    IReadOnlyList<IVideo> Videos
);