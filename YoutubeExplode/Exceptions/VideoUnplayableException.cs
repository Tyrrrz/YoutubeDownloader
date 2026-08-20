namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested video is unplayable.
/// </summary>
public class VideoUnplayableException(string message) : YoutubeExplodeException(message);
