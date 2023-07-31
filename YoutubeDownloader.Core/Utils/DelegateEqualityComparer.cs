using System;
using System.Collections.Generic;

namespace YoutubeDownloader.Core.Utils;

internal class DelegateEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _equals;
    private readonly Func<T, int> _getHashCode;

    public DelegateEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
        _equals = equals;
        _getHashCode = getHashCode;
    }

    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;

        return _equals(x, y);
    }

    public int GetHashCode(T obj) => _getHashCode(obj);
}