using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a video-only media stream.
/// </summary>
public class VideoOnlyStreamInfo(
    string url,
    Container container,
    FileSize size,
    Bitrate bitrate,
    string videoCodec,
    VideoQuality videoQuality,
    Resolution videoResolution,
    bool isVideoUpscaled
) : IVideoStreamInfo
{
    /// <summary>
    /// Initializes an instance of <see cref="VideoOnlyStreamInfo" />.
    /// </summary>
    // Backwards-compatible overload without isVideoUpscaled
    public VideoOnlyStreamInfo(
        string url,
        Container container,
        FileSize size,
        Bitrate bitrate,
        string videoCodec,
        VideoQuality videoQuality,
        Resolution videoResolution
    )
        : this(url, container, size, bitrate, videoCodec, videoQuality, videoResolution, false) { }

    /// <inheritdoc />
    public string Url { get; } = url;

    /// <inheritdoc />
    public Container Container { get; } = container;

    /// <inheritdoc />
    public FileSize Size { get; } = size;

    /// <inheritdoc />
    public Bitrate Bitrate { get; } = bitrate;

    /// <inheritdoc />
    public string VideoCodec { get; } = videoCodec;

    /// <inheritdoc />
    public VideoQuality VideoQuality { get; } = videoQuality;

    /// <inheritdoc />
    public Resolution VideoResolution { get; } = videoResolution;

    /// <inheritdoc />
    public bool IsVideoUpscaled { get; } = isVideoUpscaled;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video-only ({VideoQuality} | {Container})";
}
