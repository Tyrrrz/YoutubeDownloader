using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Utils;

internal partial class ResizableSemaphore : IDisposable
{
    private readonly object _lock = new();
    private readonly Queue<TaskCompletionSource> _waiters = new();
    private readonly CancellationTokenSource _cts = new();

    private bool _isDisposed;
    private int _maxCount = int.MaxValue;
    private int _count;

    public int MaxCount
    {
        get
        {
            lock (_lock)
            {
                return _maxCount;
            }
        }
        set
        {
            lock (_lock)
            {
                _maxCount = value;
                Refresh();
            }
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

    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        var waiter = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using (_cts.Token.Register(() => waiter.TrySetCanceled(_cts.Token)))
        await using (cancellationToken.Register(() => waiter.TrySetCanceled(cancellationToken)))
        {
            lock (_lock)
            {
                _waiters.Enqueue(waiter);
                Refresh();
            }

            await waiter.Task;

            return new AcquiredAccess(this);
        }
    }

    private void Release()
    {
        lock (_lock)
        {
            _count--;
            Refresh();
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
        _cts.Cancel();
        _cts.Dispose();
    }
}

internal partial class ResizableSemaphore
{
    private class AcquiredAccess : IDisposable
    {
        private readonly ResizableSemaphore _semaphore;

        public AcquiredAccess(ResizableSemaphore semaphore) =>
            _semaphore = semaphore;

        public void Dispose() => _semaphore.Release();
    }
}