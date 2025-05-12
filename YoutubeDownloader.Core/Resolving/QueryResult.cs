using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public record QueryResult(QueryResultKind Kind, string Title, IReadOnlyList<IVideo> Videos)
{
    public static QueryResult Aggregate(IReadOnlyList<QueryResult> results) =>
        new(
            results.Count == 1 ? results.Single().Kind : QueryResultKind.Aggregate,
            results.Count == 1 ? results.Single().Title : $"{results.Count} Queries",
            results.SelectMany(q => q.Videos).DistinctBy(v => v.Id).ToArray()
        );
}
