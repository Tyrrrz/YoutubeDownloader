namespace YoutubeExplode.Bridge;

internal interface IStreamData
{
    int? Itag { get; }

    string? Url { get; }

    string? Signature { get; }

    string? SignatureParameter { get; }

    long? ContentLength { get; }

    long? Bitrate { get; }

    string? Container { get; }

    string? AudioCodec { get; }

    string? AudioLanguageCode { get; }

    string? AudioLanguageName { get; }

    bool? IsAudioLanguageDefault { get; }

    string? VideoCodec { get; }

    string? VideoQualityLabel { get; }

    int? VideoWidth { get; }

    int? VideoHeight { get; }

    bool IsVideoUpscaled { get; }

    int? VideoFramerate { get; }
}
