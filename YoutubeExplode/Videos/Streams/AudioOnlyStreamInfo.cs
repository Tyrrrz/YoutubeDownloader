using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with an audio-only YouTube media stream.
/// </summary>
public class AudioOnlyStreamInfo(
    string url,
    Container container,
    FileSize size,
    Bitrate bitrate,
    string audioCodec,
    Language? audioLanguage,
    bool? isAudioLanguageDefault
) : IAudioStreamInfo
{
    /// <inheritdoc />
    public string Url { get; } = url;

    /// <inheritdoc />
    public Container Container { get; } = container;

    /// <inheritdoc />
    public FileSize Size { get; } = size;

    /// <inheritdoc />
    public Bitrate Bitrate { get; } = bitrate;

    /// <inheritdoc />
    public string AudioCodec { get; } = audioCodec;

    /// <inheritdoc />
    public Language? AudioLanguage { get; } = audioLanguage;

    /// <inheritdoc />
    public bool? IsAudioLanguageDefault { get; } = isAudioLanguageDefault;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() =>
        AudioLanguage is not null
            ? $"Audio-only ({Container} | {AudioLanguage})"
            : $"Audio-only ({Container})";
}
