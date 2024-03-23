using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.Views.Framework;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            var view = (Control)Activator.CreateInstance(type)!;
            return view;
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
