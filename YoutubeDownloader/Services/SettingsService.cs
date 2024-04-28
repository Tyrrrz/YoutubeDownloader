using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Platform;
using Cogwheel;
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Core.Downloading;
using Container = YoutubeExplode.Videos.Streams.Container;

namespace YoutubeDownloader.Services;

[INotifyPropertyChanged]
public partial class SettingsService()
    : SettingsBase(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.dat"))
{
    [ObservableProperty]
    private bool _isUkraineSupportMessageEnabled = true;

    [ObservableProperty]
    private bool _isAutoUpdateEnabled = true;

    [ObservableProperty]
    private bool _isDarkModeEnabled;

    [ObservableProperty]
    private bool _isAuthPersisted = true;

    [ObservableProperty]
    private bool _shouldInjectSubtitles = true;

    [ObservableProperty]
    private bool _shouldInjectTags = true;

    [ObservableProperty]
    private bool _shouldSkipExistingFiles;

    [ObservableProperty]
    private string _fileNameTemplate = "$title";

    [ObservableProperty]
    private int _parallelLimit = 2;

    [ObservableProperty]
    private IReadOnlyList<Cookie>? _lastAuthCookies;

    [ObservableProperty]
    [property: JsonConverter(typeof(ContainerJsonConverter))]
    private Container _lastContainer = Container.Mp4;

    [ObservableProperty]
    private VideoQualityPreference _lastVideoQualityPreference = VideoQualityPreference.Highest;

    public override void Reset()
    {
        base.Reset();

        // Reset the dark mode setting separately because its default value is evaluated dynamically
        // and cannot be set by the field initializer.
        IsDarkModeEnabled =
            Application.Current?.PlatformSettings?.GetColorValues().ThemeVariant
            == PlatformThemeVariant.Dark;
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
