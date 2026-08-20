using YoutubeExplode.Videos;

namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested video requires purchase.
/// </summary>
public class VideoRequiresPurchaseException(string message, VideoId previewVideoId)
    : VideoUnplayableException(message)
{
    /// <summary>
    /// ID of a free preview video which is used as promotion for the original video.
    /// </summary>
    public VideoId PreviewVideoId { get; } = previewVideoId;
}
