# YoutubeExplode.Converter

[![Version](https://img.shields.io/nuget/v/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)

**YoutubeExplode.Converter** is an extension package for **YoutubeExplode** that provides the capability to download YouTube videos into a single file by fetching individual streams and muxing them.
This package relies on [FFmpeg](https://ffmpeg.org) under the hood.

## Install

- 📦 [NuGet](https://nuget.org/packages/YoutubeExplode.Converter): `dotnet add package YoutubeExplode.Converter`

> **Important**:
> This package requires the [FFmpeg CLI](https://ffmpeg.org) to work, which can be downloaded [here](https://github.com/Tyrrrz/FFmpegBin/releases).
> Ensure that it's located in your application's probe directory or on the system's `PATH`, or provide a custom location yourself using one of the available method overloads.

> [!TIP]
> You can use [**Binternal**](https://github.com/Tyrrrz/Binternal) to internalize this library if you prefer to avoid taking an external dependency.

## Usage

**YoutubeExplode.Converter** exposes its functionality by enhancing **YoutubeExplode**'s clients with additional extension methods.
To use them, simply add the corresponding namespace and follow the examples below.

### Downloading videos

You can download a video directly to a file through one of the extension methods provided on `VideoClient`.
For example, to download a video in the specified format using the highest quality streams, simply call `DownloadAsync(...)` with the video ID and the destination path:

```csharp
using YoutubeExplode;
using YoutubeExplode.Converter;

using var youtube = new YoutubeClient();

var videoUrl = "https://youtube.com/watch?v=u_yIGGhubZs";
await youtube.Videos.DownloadAsync(videoUrl, "video.mp4");
```

Internally, this resolves the video's media streams, downloads the best candidates based on format, bitrate, framerate, and quality, and muxes them together into a single file.

> [!NOTE]
> If the specified output format is a known audio-only container (e.g., `mp3` or `ogg`) then only the audio stream is downloaded.

> [!WARNING]
> Stream muxing is a resource-intensive process, especially when transcoding is involved.
> To avoid transcoding, consider specifying either `mp4` or `webm` for the output format, as these are the containers that YouTube uses for most of its streams.

### Customizing the conversion process

To configure various aspects of the conversion process, use the following overload of `DownloadAsync(...)`:

```csharp
using YoutubeExplode;
using YoutubeExplode.Converter;

using var youtube = new YoutubeClient();
var videoUrl = "https://youtube.com/watch?v=u_yIGGhubZs";

await youtube.Videos.DownloadAsync(videoUrl, "video.mp4", o => o
    .SetContainer("webm") // override format
    .SetPreset(ConversionPreset.UltraFast) // change preset
    .SetFFmpegPath("path/to/ffmpeg") // custom FFmpeg location
    .SetEnvironmentVariable("FFREPORT", "file=ffreport.log") // custom environment variable(s) passed to FFmpeg
);
```

### Manually selecting streams

If you need precise control over which streams are used for the muxing process, you can also provide them yourself instead of relying on the automatic resolution:

```csharp
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;

using var youtube = new YoutubeClient();

// Get stream manifest
var videoUrl = "https://youtube.com/watch?v=u_yIGGhubZs";
var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

// Select best audio stream (highest bitrate)
var audioStreamInfo = streamManifest
    .GetAudioStreams()
    .Where(s => s.Container == Container.Mp4)
    .GetWithHighestBitrate();

// Select best video stream (1080p60 in this example)
var videoStreamInfo = streamManifest
    .GetVideoStreams()
    .Where(s => s.Container == Container.Mp4)
    .First(s => s.VideoQuality.Label == "1080p60");

// Download and mux streams into a single file
await youtube.Videos.DownloadAsync(
    [audioStreamInfo, videoStreamInfo],
    new ConversionRequestBuilder("video.mp4").Build()
);
```

> [!WARNING]
> Stream muxing is a resource-intensive process, especially when transcoding is involved.
> To avoid transcoding, consider prioritizing streams that are already encoded in the desired format (e.g., `mp4` or `webm`).
