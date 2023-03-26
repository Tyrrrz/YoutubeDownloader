using System;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class SettingsViewModel : DialogScreen
{
    private readonly SettingsService _settingsService;

    public bool IsAutoUpdateEnabled
    {
        get => _settingsService.IsAutoUpdateEnabled;
        set => _settingsService.IsAutoUpdateEnabled = value;
    }

    public bool IsDarkModeEnabled
    {
        get => _settingsService.IsDarkModeEnabled;
        set => _settingsService.IsDarkModeEnabled = value;
    }

    public bool ShouldInjectTags
    {
        get => _settingsService.ShouldInjectTags;
        set => _settingsService.ShouldInjectTags = value;
    }

    public bool ShouldDownloadThumbnail
    {
        get => _settingsService.ShouldDownloadThumbnail;
        set => _settingsService.ShouldDownloadThumbnail = value;
    }

    public bool ShouldDownloadClosedCaption
    {
        get => _settingsService.ShouldDownloadClosedCaptions;
        set => _settingsService.ShouldDownloadClosedCaptions = value;
    }

    public bool ShouldSkipExistingFiles
    {
        get => _settingsService.ShouldSkipExistingFiles;
        set => _settingsService.ShouldSkipExistingFiles = value;
    }

    public string FileNameTemplate
    {
        get => _settingsService.FileNameTemplate;
        set => _settingsService.FileNameTemplate = value;
    }

    public string TranslateKey
    {
        get => _settingsService.TranslateKey;
        set => _settingsService.TranslateKey = value;
    }

    public int ParallelLimit
    {
        get => _settingsService.ParallelLimit;
        set => _settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }
}