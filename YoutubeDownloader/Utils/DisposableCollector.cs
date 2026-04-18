using System;
using System.Collections.Generic;
using PowerKit;

namespace YoutubeDownloader.Utils;

internal class DisposableCollector : IDisposable
{
    private readonly object _lock = new();
    private readonly List<IDisposable> _items = [];

    public void Add(IDisposable item)
    {
        lock (_lock)
        {
            _items.Add(item);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            Disposable.Merge(_items).Dispose();
            _items.Clear();
        }
    }
}
