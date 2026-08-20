using System;

namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown within <see cref="YoutubeExplode" />.
/// </summary>
public class YoutubeExplodeException(string message) : Exception(message);
