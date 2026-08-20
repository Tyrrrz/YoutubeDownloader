namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested video is unavailable.
/// </summary>
public class VideoUnavailableException(string message) : VideoUnplayableException(message);
