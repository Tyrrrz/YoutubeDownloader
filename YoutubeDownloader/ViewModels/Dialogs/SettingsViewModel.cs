using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    private readonly DisposableCollector _eventRoot = new();

    public SettingsViewModel(
        DialogManager dialogManager,
        LocalizationManager localizationManager,
        SettingsService settingsService
    )
    {
        _dialogManager = dialogManager;
        LocalizationManager = localizationManager;
        _settingsService = settingsService;

        _eventRoot.Add(_settingsService.WatchAllProperties(OnAllPropertiesChanged));
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
            _eventRoot.Dispose();
        }

        base.Dispose(disposing);
    }
}
