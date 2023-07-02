using System.Collections.Generic;

namespace YoutubeDownloader.Core.Utils.Extensions;

internal static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var i in items)
            source.Add(i);
    }
    
    public static string Join<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);
}