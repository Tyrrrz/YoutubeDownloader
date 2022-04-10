using System;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Utils;

internal class ThrottleLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly TimeSpan _interval;
    private DateTimeOffset _lastRequestInstant = DateTimeOffset.MinValue;

    public ThrottleLock(TimeSpan interval) =>
        _interval = interval;

    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var timePassedSinceLastRequest = DateTimeOffset.Now - _lastRequestInstant;

            var remainingTime = _interval - timePassedSinceLastRequest;
            if (remainingTime > TimeSpan.Zero)
                await Task.Delay(remainingTime, cancellationToken);

            _lastRequestInstant = DateTimeOffset.Now;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose() => _semaphore.Dispose();
}