using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YoutubeDownloader.ViewModels.Framework;

public partial class ViewModelBase : ObservableObject, IViewAware
{
    private Control? _view;

    protected virtual void OnViewLoaded() { }

    protected virtual void OnClose() { }

    void IViewAware.AttachView(Control view)
    {
        if (_view == view)
        {
            return; // Already attached
        }

        _view = view;

        view.Loaded += OnViewLoaded;
        view.Unloaded += OnViewUnloaded;

        if (view.IsLoaded)
        {
            OnViewLoaded();
        }
    }

    private void OnViewUnloaded(object? sender, EventArgs e)
    {
        if (_view != null)
        {
            _view.Loaded -= OnViewLoaded;
            _view.Unloaded -= OnViewUnloaded;
        }

        OnClose();
    }

    private void OnViewLoaded(object? sender, EventArgs e)
    {
        OnViewLoaded();
    }
}

public interface IViewAware
{
    void AttachView(Control control);
}
