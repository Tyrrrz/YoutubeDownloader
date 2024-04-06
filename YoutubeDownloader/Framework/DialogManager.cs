using System;
using System.Threading;
using System.Threading.Tasks;
using DialogHostAvalonia;

namespace YoutubeDownloader.Framework;

public class DialogManager : IDisposable
{
    private readonly SemaphoreSlim _dialogLock = new(1, 1);

    public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialog)
    {
        await _dialogLock.WaitAsync();
        try
        {
            await DialogHost.Show(
                dialog,
                // It's fine to await in a void method here because it's an event handler
                // ReSharper disable once AsyncVoidLambda
                async (object _, DialogOpenedEventArgs args) =>
                {
                    await dialog.WaitForCloseAsync();

                    try
                    {
                        args.Session.Close();
                    }
                    catch (InvalidOperationException)
                    {
                        // Dialog host is already processing a close operation
                    }
                }
            );

            return dialog.DialogResult;
        }
        finally
        {
            _dialogLock.Release();
        }
    }

    public void Dispose() => _dialogLock.Dispose();
}
