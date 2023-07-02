using System.Collections.Generic;

namespace YoutubeDownloader.Core.Utils.Extensions;

internal static class DictionaryExtensions
{
    public static TValue? TryGetValue<TKey,TValue>(this Dictionary<TKey,TValue> source, TKey key) where TKey : notnull
    {
        source.TryGetValue(key, out var value);
        return value;
    }
}