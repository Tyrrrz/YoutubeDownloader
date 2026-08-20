using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Metadata associated with a media stream of a YouTube video.
/// </summary>
public interface IStreamInfo
{
    /// <summary>
    /// Stream URL.
    /// </summary>
    /// <remarks>
    /// While this URL can be used to access the underlying stream, you need a series
    /// of carefully crafted HTTP requests in order to do so.
    /// It's highly recommended to use
    /// <see cref="StreamClient.GetAsync" /> or <see cref="StreamClient.DownloadAsync" />
    /// instead, as they will perform all the heavy lifting for you.
    /// </remarks>
    string Url { get; }

    /// <summary>
    /// Stream container.
    /// </summary>
    Container Container { get; }

    /// <summary>
    /// Stream size.
    /// </summary>
    FileSize Size { get; }

    /// <summary>
    /// Stream bitrate.
    /// </summary>
    Bitrate Bitrate { get; }
}

/// <summary>
/// Extensions for <see cref="IStreamInfo" />.
/// </summary>
public static class StreamInfoExtensions
{
    /// <inheritdoc cref="StreamInfoExtensions" />
    extension(IStreamInfo streamInfo)
    {
        internal bool IsThrottled() =>
            !string.Equals(
                UrlEx.TryGetQueryParameterValue(streamInfo.Url, "ratebypass"),
                "yes",
                StringComparison.OrdinalIgnoreCase
            );
    }

    /// <inheritdoc cref="StreamInfoExtensions" />
    extension(IEnumerable<IStreamInfo> streamInfos)
    {
        /// <summary>
        /// Gets the stream with the highest bitrate.
        /// Returns <see langword="null" /> if the sequence is empty.
        /// </summary>
        public IStreamInfo? TryGetWithHighestBitrate() => streamInfos.MaxBy(s => s.Bitrate);

        /// <summary>
        /// Gets the stream with the highest bitrate.
        /// </summary>
        public IStreamInfo GetWithHighestBitrate() =>
            streamInfos.TryGetWithHighestBitrate()
            ?? throw new InvalidOperationException("Input stream collection is empty.");
    }
}
