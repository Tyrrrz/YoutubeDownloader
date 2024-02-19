using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeDownloader.Views.Framework;

public interface IViewManager
{
    Control? CreateAndBindViewForModelIfNecessary(object? model);
    public TopLevel? GetTopLevel();
}

public class ViewManager : IViewManager
{
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly ViewLocator _viewLocator = new();

    public ViewManager(IApplicationLifetime applicationLifetime)
    {
        _applicationLifetime = applicationLifetime;
    }

    public Control? CreateAndBindViewForModelIfNecessary(object? model)
    {
        var view = _viewLocator.Match(model) ? _viewLocator.Build(model) : null;

        if (view != null)
        {
            view.DataContext = model;
        }

        return view;
    }

    public TopLevel? GetTopLevel()
    {
        if (
            _applicationLifetime is IClassicDesktopStyleApplicationLifetime
            {
                MainWindow: { } window
            }
        )
        {
            return window;
        }

        if (_applicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView })
        {
            var visualRoot = mainView.GetVisualRoot();
            if (visualRoot is TopLevel topLevel)
            {
                return topLevel;
            }
        }

        return null;
    }
}
