using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views;
using YoutubeDownloader.Views.Components;
using YoutubeDownloader.Views.Dialogs;

namespace YoutubeDownloader.Framework;

public partial class ViewManager
{
    private static readonly IReadOnlyDictionary<Type, Type> ViewModelTypeToViewTypeMap =
        new Dictionary<Type, Type>
        {
            [typeof(DashboardViewModel)] = typeof(DashboardView),
            [typeof(AuthSetupViewModel)] = typeof(AuthSetupView),
            [typeof(DownloadMultipleSetupViewModel)] = typeof(DownloadMultipleSetupView),
            [typeof(DownloadSingleSetupViewModel)] = typeof(DownloadSingleSetupView),
            [typeof(MessageBoxViewModel)] = typeof(MessageBoxView),
            [typeof(SettingsViewModel)] = typeof(SettingsView),
            [typeof(MainViewModel)] = typeof(MainView)
        };

    public Control? TryBindView(ViewModelBase viewModel)
    {
        if (!ViewModelTypeToViewTypeMap.TryGetValue(viewModel.GetType(), out var type))
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
