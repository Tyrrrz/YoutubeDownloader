using System;
using System.Globalization;
using System.Text.RegularExpressions;
using PowerKit.Extensions;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Video stream quality.
/// </summary>
public readonly partial struct VideoQuality(string label, int maxHeight, int framerate)
{
    /// <summary>
    /// Initializes an instance of <see cref="VideoQuality" />.
    /// </summary>
    public VideoQuality(int maxHeight, int framerate)
        : this(FormatLabel(maxHeight, framerate), maxHeight, framerate) { }

    /// <summary>
    /// Quality label, as seen on YouTube (e.g., 1080p, 720p60, etc).
    /// </summary>
    public string Label { get; } = label;

    /// <summary>
    /// Maximum video height allowed by this quality (e.g., 1080 for 1080p60).
    /// Actual video height may be lower in some cases.
    /// </summary>
    public int MaxHeight { get; } = maxHeight;

    /// <summary>
    /// Video framerate, measured in frames per second.
    /// </summary>
    public int Framerate { get; } = framerate;

    /// <summary>
    /// Whether this is a high definition video (i.e., 1080p or above).
    /// </summary>
    public bool IsHighDefinition => MaxHeight >= 1080;

    internal Resolution GetDefaultVideoResolution() =>
        MaxHeight switch
        {
            144 => new Resolution(256, 144),
            240 => new Resolution(426, 240),
            360 => new Resolution(640, 360),
            480 => new Resolution(854, 480),
            720 => new Resolution(1280, 720),
            1080 => new Resolution(1920, 1080),
            1440 => new Resolution(2560, 1440),
            2160 => new Resolution(3840, 2160),
            2880 => new Resolution(5120, 2880),
            3072 => new Resolution(4096, 3072),
            4320 => new Resolution(7680, 4320),
            _ => new Resolution(16 * MaxHeight / 9, MaxHeight),
        };

    /// <inheritdoc />
    public override string ToString() => Label;
}

public partial struct VideoQuality
{
    private static string FormatLabel(int maxHeight, int framerate)
    {
        // Framerate appears only if it's above 30
        if (framerate <= 30)
            return $"{maxHeight}p";

        // YouTube rounds framerate to the next nearest decimal
        var framerateRounded = (int)Math.Ceiling(framerate / 10.0) * 10;
        return $"{maxHeight}p{framerateRounded}";
    }

    internal static VideoQuality FromLabel(string label, int framerateFallback)
    {
        // Video quality labels can have the following formats:
        // - 1080p (regular stream, regular fps)
        // - 1080p60 (regular stream, high fps)
        // - 1080s (360° stream, regular fps)
        // - 1080s60 (360° stream, high fps)
        // - 2160p60 HDR (high dynamic range, high fps)

        var match = Regex.Match(label, @"^(\d+)\D(\d+)?");

        var maxHeight = match.Groups[1].Value.Pipe(s => int.Parse(s, CultureInfo.InvariantCulture));

        var framerate = match
            .Groups[2]
            .Value.NullIfWhiteSpace()
            ?.Pipe(s => int.ParseOrNull(s, CultureInfo.InvariantCulture));

        return new VideoQuality(label, maxHeight, framerate ?? framerateFallback);
    }

    internal static VideoQuality FromItag(int itag, int framerate)
    {
        var maxHeight = itag switch
        {
            5 => 144,
            6 => 240,
            13 => 144,
            17 => 144,
            18 => 360,
            22 => 720,
            34 => 360,
            35 => 480,
            36 => 240,
            37 => 1080,
            38 => 3072,
            43 => 360,
            44 => 480,
            45 => 720,
            46 => 1080,
            59 => 480,
            78 => 480,
            82 => 360,
            83 => 480,
            84 => 720,
            85 => 1080,
            91 => 144,
            92 => 240,
            93 => 360,
            94 => 480,
            95 => 720,
            96 => 1080,
            100 => 360,
            101 => 480,
            102 => 720,
            132 => 240,
            151 => 144,
            133 => 240,
            134 => 360,
            135 => 480,
            136 => 720,
            137 => 1080,
            138 => 4320,
            142 => 240,
            143 => 360,
            144 => 480,
            145 => 720,
            146 => 1080,
            160 => 144,
            161 => 144,
            167 => 360,
            168 => 480,
            169 => 720,
            170 => 1080,
            212 => 480,
            213 => 480,
            214 => 720,
            215 => 720,
            216 => 1080,
            217 => 1080,
            218 => 480,
            219 => 480,
            222 => 480,
            223 => 480,
            224 => 720,
            225 => 720,
            226 => 1080,
            227 => 1080,
            242 => 240,
            243 => 360,
            244 => 480,
            245 => 480,
            246 => 480,
            247 => 720,
            248 => 1080,
            264 => 1440,
            266 => 2160,
            271 => 1440,
            272 => 2160,
            278 => 144,
            298 => 720,
            299 => 1080,
            302 => 720,
            303 => 1080,
            308 => 1440,
            313 => 2160,
            315 => 2160,
            330 => 144,
            331 => 240,
            332 => 360,
            333 => 480,
            334 => 720,
            335 => 1080,
            336 => 1440,
            337 => 2160,
            399 => 1080,
            398 => 720,
            397 => 480,
            396 => 360,
            395 => 240,
            394 => 144,
            _ => throw new ArgumentException($"Unrecognized itag '{itag}'.", nameof(itag)),
        };

        return new VideoQuality(maxHeight, framerate);
    }
}

public partial struct VideoQuality : IComparable<VideoQuality>, IEquatable<VideoQuality>
{
    /// <inheritdoc />
    public int CompareTo(VideoQuality other)
    {
        var maxHeightComparison = MaxHeight.CompareTo(other.MaxHeight);
        if (maxHeightComparison != 0)
            return maxHeightComparison;

        var framerateComparison = Framerate.CompareTo(other.Framerate);
        if (framerateComparison != 0)
            return framerateComparison;

        return string.Compare(Label, other.Label, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool Equals(VideoQuality other) =>
        string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase)
        && MaxHeight == other.MaxHeight
        && Framerate == other.Framerate;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is VideoQuality other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(
            Label.GetHashCode(StringComparison.OrdinalIgnoreCase),
            MaxHeight,
            Framerate
        );

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(VideoQuality left, VideoQuality right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(VideoQuality, VideoQuality)" />
    public static bool operator !=(VideoQuality left, VideoQuality right) => !(left == right);

    /// <summary>
    /// Comparison.
    /// </summary>
    public static bool operator >(VideoQuality left, VideoQuality right) =>
        left.CompareTo(right) > 0;

    /// <inheritdoc cref="operator >(VideoQuality, VideoQuality)" />
    public static bool operator <(VideoQuality left, VideoQuality right) =>
        left.CompareTo(right) < 0;
}
