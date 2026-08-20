using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Describes media streams available for a YouTube video.
/// </summary>
public class StreamManifest(IReadOnlyList<IStreamInfo> streams)
{
    /// <summary>
    /// Available streams.
    /// </summary>
    public IReadOnlyList<IStreamInfo> Streams { get; } = streams;

    /// <summary>
    /// Gets streams that contain audio (i.e., muxed and audio-only streams).
    /// </summary>
    public IEnumerable<IAudioStreamInfo> GetAudioStreams() => Streams.OfType<IAudioStreamInfo>();

    /// <summary>
    /// Gets streams that contain video (i.e., muxed and video-only streams).
    /// </summary>
    public IEnumerable<IVideoStreamInfo> GetVideoStreams() => Streams.OfType<IVideoStreamInfo>();

    /// <summary>
    /// Gets muxed streams (i.e., streams containing both audio and video).
    /// </summary>
    /// <remarks>
    /// These streams are generally deprecated by YouTube and may not be available
    /// for every video. If needed, use the YoutubeExplode.Converter package to
    /// manually mux audio-only and video-only streams into a single container.
    /// </remarks>
    public IEnumerable<MuxedStreamInfo> GetMuxedStreams() => Streams.OfType<MuxedStreamInfo>();

    /// <summary>
    /// Gets audio-only streams.
    /// </summary>
    public IEnumerable<AudioOnlyStreamInfo> GetAudioOnlyStreams() =>
        GetAudioStreams().OfType<AudioOnlyStreamInfo>();

    /// <summary>
    /// Gets video-only streams.
    /// </summary>
    public IEnumerable<VideoOnlyStreamInfo> GetVideoOnlyStreams() =>
        GetVideoStreams().OfType<VideoOnlyStreamInfo>();
}
