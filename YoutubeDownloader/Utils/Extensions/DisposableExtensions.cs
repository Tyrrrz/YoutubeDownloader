using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeDownloader.Utils.Extensions;

internal static class DisposableExtensions
{
    public static void DisposeAll(this IEnumerable<IDisposable> disposables)
    {
        var exceptions = default(List<Exception>);

        foreach (var disposable in disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                (exceptions ??= []).Add(ex);
            }
        }

        if (exceptions?.Any() == true)
            throw new AggregateException(exceptions);
    }
}
