using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using PowerKit.Extensions;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    private readonly IDisposable _eventSubscription;

    public SettingsViewModel(
        DialogManager dialogManager,
        LocalizationManager localizationManager,
        SettingsService settingsService
    )
    {
        _dialogManager = dialogManager;
        LocalizationManager = localizationManager;
        _settingsService = settingsService;

        _eventSubscription = _settingsService.WatchAllProperties(OnAllPropertiesChanged);
    }

    public LocalizationManager LocalizationManager { get; }

    public IReadOnlyList<ThemeVariant> AvailableThemes { get; } = Enum.GetValues<ThemeVariant>();

    public ThemeVariant Theme
    {
        get => _settingsService.Theme;
        set => _settingsService.Theme = value;
    }

    public IReadOnlyList<Language> AvailableLanguages { get; } = Enum.GetValues<Language>();

    public Language Language
    {
        get => _settingsService.Language;
        set => _settingsService.Language = value;
    }

    public bool IsAutoUpdateAvailable { get; } =
        OperatingSystem.IsWindows() && StartOptions.Current.IsAutoUpdateAllowed;

    public bool IsAutoUpdateEnabled
    {
        get => _settingsService.IsAutoUpdateEnabled;
        set => _settingsService.IsAutoUpdateEnabled = value;
    }

    public bool IsAuthPersisted
    {
        get => _settingsService.IsAuthPersisted;
        set => _settingsService.IsAuthPersisted = value;
    }

    public string? FFmpegFilePath
    {
        get => _settingsService.FFmpegFilePath;
        set => _settingsService.FFmpegFilePath = !string.IsNullOrWhiteSpace(value) ? value : null;
    }

    public bool ShouldInjectLanguageSpecificAudioStreams
    {
        get => _settingsService.ShouldInjectLanguageSpecificAudioStreams;
        set => _settingsService.ShouldInjectLanguageSpecificAudioStreams = value;
    }

    public bool ShouldInjectSubtitles
    {
        get => _settingsService.ShouldInjectSubtitles;
        set => _settingsService.ShouldInjectSubtitles = value;
    }

    public bool ShouldInjectTags
    {
        get => _settingsService.ShouldInjectTags;
        set => _settingsService.ShouldInjectTags = value;
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

    public int ParallelLimit
    {
        get => _settingsService.ParallelLimit;
        set => _settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }

    [RelayCommand]
    private async Task BrowseFFmpegFilePathAsync()
    {
        var fileTypes = OperatingSystem.IsWindows()
            ? new[]
            {
                new FilePickerFileType("FFmpeg executable") { Patterns = ["*.exe"] },
                FilePickerFileTypes.All,
            }
            : null;

        var filePath = await _dialogManager.PromptOpenFilePathAsync(fileTypes);

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        FFmpegFilePath = filePath;
    }

    [RelayCommand]
    private void ResetFFmpegFilePath() => FFmpegFilePath = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventSubscription.Dispose();
        }

        base.Dispose(disposing);
    }
}
