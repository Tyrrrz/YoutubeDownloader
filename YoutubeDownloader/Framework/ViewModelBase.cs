using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YoutubeDownloader.Framework;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    ~ViewModelBase() => Dispose(false);

    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    public virtual Task InitializeAsync() => Task.CompletedTask;

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
