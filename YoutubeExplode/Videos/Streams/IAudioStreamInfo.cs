using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a media stream that contains audio.
/// </summary>
public interface IAudioStreamInfo : IStreamInfo
{
    /// <summary>
    /// Audio codec.
    /// </summary>
    string AudioCodec { get; }

    /// <summary>
    /// Audio language.
    /// </summary>
    /// <remarks>
    /// May be null if the audio stream does not contain language information.
    /// </remarks>
    Language? AudioLanguage { get; }

    /// <summary>
    /// Whether the audio stream's language corresponds to the default language of the video.
    /// </summary>
    /// <remarks>
    /// May be null if the audio stream does not contain language information.
    /// </remarks>
    bool? IsAudioLanguageDefault { get; }
}
