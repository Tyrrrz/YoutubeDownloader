using System;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class SettingsViewModel(SettingsService settingsService) : DialogScreen
{
    public bool IsAutoUpdateEnabled
    {
        get => settingsService.IsAutoUpdateEnabled;
        set => settingsService.IsAutoUpdateEnabled = value;
    }

    public bool IsDarkModeEnabled
    {
        get => settingsService.IsDarkModeEnabled;
        set => settingsService.IsDarkModeEnabled = value;
    }

    public bool IsAuthPersisted
    {
        get => settingsService.IsAuthPersisted;
        set => settingsService.IsAuthPersisted = value;
    }

    public bool ShouldInjectSubtitles
    {
        get => settingsService.ShouldInjectSubtitles;
        set => settingsService.ShouldInjectSubtitles = value;
    }

    public bool ShouldInjectTags
    {
        get => settingsService.ShouldInjectTags;
        set => settingsService.ShouldInjectTags = value;
    }

    public bool ShouldSkipExistingFiles
    {
        get => settingsService.ShouldSkipExistingFiles;
        set => settingsService.ShouldSkipExistingFiles = value;
    }

    public string FileNameTemplate
    {
        get => settingsService.FileNameTemplate;
        set => settingsService.FileNameTemplate = value;
    }

    public int ParallelLimit
    {
        get => settingsService.ParallelLimit;
        set => settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }
}
