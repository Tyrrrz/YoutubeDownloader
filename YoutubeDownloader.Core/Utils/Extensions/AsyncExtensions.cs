using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Utils.Extensions;

internal static class AsyncExtensions
{
    private static async ValueTask<IReadOnlyList<T>> AggregateAsync<T>(
        this IAsyncEnumerable<T> asyncEnumerable)
    {
        var list = new List<T>();

        await foreach (var i in asyncEnumerable)
            list.Add(i);

        return list;
    }

    public static ValueTaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>(
        this IAsyncEnumerable<T> asyncEnumerable) =>
        asyncEnumerable.AggregateAsync().GetAwaiter();
}