using System.Collections.Generic;

namespace YoutubeDownloader.Core.Utils.Extensions;

public static class CollectionExtensions
{
    extension<T>(ICollection<T> source)
    {
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var i in items)
                source.Add(i);
        }
    }
}
