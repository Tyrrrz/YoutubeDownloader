using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Utils;

// Like a regular semaphore, but the max count can be changed at any point
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
            // Provide access to pending waiters, as long as max count allows
            while (_count < MaxCount && _waiters.TryDequeue(out var waiter))
            {
                // Don't increment the count if the waiter has already been
                // completed before (most likely by getting canceled).
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
            // Add the waiter to the queue
            lock (_lock)
            {
                _waiters.Enqueue(waiter);
                Refresh();
            }

            // Wait until this waiter has been given access
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
        if (!_isDisposed)
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        _isDisposed = true;
    }
}

internal partial class ResizableSemaphore
{
    private class AcquiredAccess(ResizableSemaphore semaphore) : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                semaphore.Release();
            }

            _isDisposed = true;
        }
    }
}
