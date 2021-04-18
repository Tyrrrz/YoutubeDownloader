using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeDownloader.Utils.Extensions
{
    internal static class CollectionExtensions
    {
        public static void RemoveWhere<T>(this ICollection<T> source, Predicate<T> predicate)
        {
            foreach (var i in source.ToArray())
            {
                if (predicate(i))
                    source.Remove(i);
            }
        }
    }
}