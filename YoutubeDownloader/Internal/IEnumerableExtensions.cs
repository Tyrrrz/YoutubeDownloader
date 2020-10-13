using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Domain.Internal.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        public static bool Includes<T>(this IEnumerable<T> source, T searchElement)
        {
            return Any(source, (element) => element != null && element.Equals(searchElement));
        }

        public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T item in source)
            {
                if (predicate(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
