using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cogwheel;
using Microsoft.Win32;
using PropertyChanged;
using YoutubeDownloader.Core.Downloading;
using Container = YoutubeExplode.Videos.Streams.Container;

namespace YoutubeDownloader.Services;

[AddINotifyPropertyChangedInterface]
public partial class SettingsService : SettingsBase, INotifyPropertyChanged
{
    public bool IsUkraineSupportMessageEnabled { get; set; } = true;

    public bool IsAutoUpdateEnabled { get; set; } = true;

    public bool IsDarkModeEnabled { get; set; } = IsDarkModeEnabledByDefault();

    public bool ShouldInjectTags { get; set; } = true;

    public bool ShouldSkipExistingFiles { get; set; }

    public string FileNameTemplate { get; set; } = "$title";

    public int ParallelLimit { get; set; } = 2;

    public Version? LastAppVersion { get; set; }

    // STJ cannot properly serialize immutable structs
    [JsonConverter(typeof(ContainerJsonConverter))]
    public Container LastContainer { get; set; } = Container.Mp4;

    public VideoQualityPreference LastVideoQualityPreference { get; set; } = VideoQualityPreference.Highest;

    public SettingsService()
        : base(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.dat"))
    {
    }
}

public partial class SettingsService
{
    private static bool IsDarkModeEnabledByDefault()
    {
        try
        {
            return Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
                false
            )?.GetValue("AppsUseLightTheme") is 0;
        }
        catch
        {
            return false;
        }
    }
}

public partial class SettingsService
{
    private class ContainerJsonConverter : JsonConverter<Container>
    {
        public override Container Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Container? result = null;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName &&
                        reader.GetString() == "Name" &&
                        reader.Read() &&
                        reader.TokenType == JsonTokenType.String)
                    {
                        var name = reader.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                            result = new Container(name);
                    }
                }
            }

            return result ?? throw new InvalidOperationException($"Invalid JSON for type '{typeToConvert.FullName}'.");
        }

        public override void Write(Utf8JsonWriter writer, Container value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", value.Name);
            writer.WriteEndObject();
        }
    }
}