using System;
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Localization;

namespace YoutubeDownloader.Framework;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    public LocalizedStrings Localization => LocalizedStrings.Current;

    ~ViewModelBase() => Dispose(false);

    protected void OnAllPropertiesChanged() => OnPropertyChanged(string.Empty);

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
