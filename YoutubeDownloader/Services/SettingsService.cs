using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using Container = YoutubeExplode.Videos.Streams.Container;

namespace YoutubeDownloader.Services;

// Can't use [ObservableProperty] here because System.Text.Json's source generator doesn't see
// the generated properties.
[INotifyPropertyChanged]
public partial class SettingsService()
    : SettingsBase(
        Path.Combine(AppContext.BaseDirectory, "Settings.dat"),
        SerializerContext.Default
    )
{
    private bool _isUkraineSupportMessageEnabled = true;
    public bool IsUkraineSupportMessageEnabled
    {
        get => _isUkraineSupportMessageEnabled;
        set => SetProperty(ref _isUkraineSupportMessageEnabled, value);
    }

    private ThemeVariant _theme;
    public ThemeVariant Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    private bool _isAutoUpdateEnabled = true;
    public bool IsAutoUpdateEnabled
    {
        get => _isAutoUpdateEnabled;
        set => SetProperty(ref _isAutoUpdateEnabled, value);
    }

    private bool _isAuthPersisted = true;
    public bool IsAuthPersisted
    {
        get => _isAuthPersisted;
        set => SetProperty(ref _isAuthPersisted, value);
    }

    private bool _shouldInjectLanguageSpecificAudioStreams = true;
    public bool ShouldInjectLanguageSpecificAudioStreams
    {
        get => _shouldInjectLanguageSpecificAudioStreams;
        set => SetProperty(ref _shouldInjectLanguageSpecificAudioStreams, value);
    }

    private bool _shouldInjectSubtitles = true;
    public bool ShouldInjectSubtitles
    {
        get => _shouldInjectSubtitles;
        set => SetProperty(ref _shouldInjectSubtitles, value);
    }

    private bool _shouldInjectTags = true;
    public bool ShouldInjectTags
    {
        get => _shouldInjectTags;
        set => SetProperty(ref _shouldInjectTags, value);
    }

    private bool _shouldSkipExistingFiles;
    public bool ShouldSkipExistingFiles
    {
        get => _shouldSkipExistingFiles;
        set => SetProperty(ref _shouldSkipExistingFiles, value);
    }

    private string _fileNameTemplate = "$title";
    public string FileNameTemplate
    {
        get => _fileNameTemplate;
        set => SetProperty(ref _fileNameTemplate, value);
    }

    private int _parallelLimit = 2;
    public int ParallelLimit
    {
        get => _parallelLimit;
        set => SetProperty(ref _parallelLimit, value);
    }

    private IReadOnlyList<Cookie>? _lastAuthCookies;
    public IReadOnlyList<Cookie>? LastAuthCookies
    {
        get => _lastAuthCookies;
        set => SetProperty(ref _lastAuthCookies, value);
    }

    private Container _lastContainer = Container.Mp4;

    [JsonConverter(typeof(ContainerJsonConverter))]
    public Container LastContainer
    {
        get => _lastContainer;
        set => SetProperty(ref _lastContainer, value);
    }

    private VideoQualityPreference _lastVideoQualityPreference = VideoQualityPreference.Highest;
    public VideoQualityPreference LastVideoQualityPreference
    {
        get => _lastVideoQualityPreference;
        set => SetProperty(ref _lastVideoQualityPreference, value);
    }

    public override void Save()
    {
        // Clear the cookies if they are not supposed to be persisted
        var lastAuthCookies = LastAuthCookies;
        if (!IsAuthPersisted)
            LastAuthCookies = null;

        base.Save();

        LastAuthCookies = lastAuthCookies;
    }
}

public partial class SettingsService
{
    private class ContainerJsonConverter : JsonConverter<Container>
    {
        public override Container Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            Container? result = null;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (
                        reader.TokenType == JsonTokenType.PropertyName
                        && reader.GetString() == "Name"
                        && reader.Read()
                        && reader.TokenType == JsonTokenType.String
                    )
                    {
                        var name = reader.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                            result = new Container(name);
                    }
                }
            }

            return result
                ?? throw new InvalidOperationException(
                    $"Invalid JSON for type '{typeToConvert.FullName}'."
                );
        }

        public override void Write(
            Utf8JsonWriter writer,
            Container value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();
            writer.WriteString("Name", value.Name);
            writer.WriteEndObject();
        }
    }
}

public partial class SettingsService
{
    [JsonSerializable(typeof(SettingsService))]
    private partial class SerializerContext : JsonSerializerContext;
}
