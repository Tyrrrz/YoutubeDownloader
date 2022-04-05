using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Utils;

internal class ResizableSemaphore : IDisposable
{
    private readonly object _lock = new();
    private readonly Queue<TaskCompletionSource> _waiters = new();
    private readonly CancellationTokenSource _cts = new();

    private bool _isDisposed;
    private int _maxCount = int.MaxValue;
    private int _count;

    public int MaxCount
    {
        get => _maxCount;
        set
        {
            _maxCount = value;
            Refresh();
        }
    }

    private void Refresh()
    {
        lock (_lock)
        {
            while (_count < MaxCount && _waiters.TryDequeue(out var waiter))
            {
                // Don't increment if the waiter has ben canceled
                if (waiter.TrySetResult())
                    _count++;
            }
        }
    }

    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        var waiter = new TaskCompletionSource();

        await using (_cts.Token.Register(() => waiter.TrySetCanceled(_cts.Token)))
        await using (cancellationToken.Register(() => waiter.TrySetCanceled(cancellationToken)))
        {
            lock (_lock)
            {
                _waiters.Enqueue(waiter);
                Refresh();
            }

            await waiter.Task;
        }
    }

    public void Release()
    {
        lock (_lock)
        {
            _count--;
            Refresh();
        }
    }

    public async Task WrapAsync(Func<Task> getTask, CancellationToken cancellationToken = default)
    {
        await WaitAsync(cancellationToken);

        try
        {
            await getTask();
        }
        finally
        {
            Release();
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
        _cts.Cancel();
        _cts.Dispose();
    }
}