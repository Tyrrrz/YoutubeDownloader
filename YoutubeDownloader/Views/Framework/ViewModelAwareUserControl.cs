using System;
using Avalonia.Controls;
using PropertyChanged;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.Views.Framework;

[DoNotNotify]
public class ViewModelAwareUserControl<TViewModel> : UserControl
    where TViewModel : ViewModelBase
{
    protected TViewModel ViewModel => (TViewModel)DataContext!;

    protected ViewModelAwareUserControl()
    {
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is IViewAware viewAware)
        {
            viewAware.AttachView(this);
        }
    }
}
