using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeDownloader.Views.Framework;

public interface IViewManager
{
    Control? CreateAndBindViewForModelIfNecessary(object? model);
    public TopLevel? GetTopLevel();
}

public class ViewManager(IApplicationLifetime applicationLifetime) : IViewManager
{
    private readonly ViewLocator _viewLocator = new();

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
            applicationLifetime is IClassicDesktopStyleApplicationLifetime
            {
                MainWindow: { } window
            }
        )
        {
            return window;
        }

        if (applicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView })
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
