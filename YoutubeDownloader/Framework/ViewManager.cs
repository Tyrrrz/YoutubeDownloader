using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace YoutubeDownloader.Framework;

public partial class ViewManager
{
    public Control? TryBindView(ViewModelBase viewModel)
    {
        var name = viewModel
            .GetType()
            .FullName?.Replace("ViewModel", "View", StringComparison.Ordinal);

        if (string.IsNullOrWhiteSpace(name))
            return null;

        var type = Type.GetType(name);
        if (type is null)
            return null;

        if (Activator.CreateInstance(type) is not Control view)
            return null;

        view.DataContext ??= viewModel;

        return view;
    }
}

public partial class ViewManager : IDataTemplate
{
    bool IDataTemplate.Match(object? data) => data is ViewModelBase;

    Control? ITemplate<object?, Control?>.Build(object? data) =>
        data is ViewModelBase viewModel ? TryBindView(viewModel) : null;
}
