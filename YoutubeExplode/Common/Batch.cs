using System.Collections.Generic;
using PowerKit.Extensions;

namespace YoutubeExplode.Common;

/// <summary>
/// Generic collection of items returned by a single request.
/// </summary>
public class Batch<T>(IReadOnlyList<T> items)
    where T : IBatchItem
{
    /// <summary>
    /// Items included in the batch.
    /// </summary>
    public IReadOnlyList<T> Items { get; } = items;
}

internal static class Batch
{
    public static Batch<T> Create<T>(IReadOnlyList<T> items)
        where T : IBatchItem => new(items);
}

internal static class BatchExtensions
{
    extension<T>(IAsyncEnumerable<Batch<T>> source)
        where T : IBatchItem
    {
        public IAsyncEnumerable<T> FlattenAsync() => source.SelectManyAsync(b => b.Items);
    }
}
