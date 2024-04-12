using System;

namespace YoutubeDownloader.Utils;

internal class Disposable(Action dispose) : IDisposable
{
    public static IDisposable Create(Action dispose) => new Disposable(dispose);

    public void Dispose() => dispose();
}
