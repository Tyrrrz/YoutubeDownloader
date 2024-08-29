# Contributing

## Building from source

### Requirements

- Windows, Linux, or macOS machine
- Latest [.NET SDK](https://dotnet.microsoft.com/download)
- (Recommended) Latest [PowerShell (`pwsh`)](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- (Recommended) .NET IDE such as Visual Studio or JetBrains Rider

### Bootstrapping FFmpeg

When working on the project locally, you can either use a global instance of FFmpeg (i.e. found on the system's `PATH`) or a local instance (found in the project directory).
To use a local instance, place the corresponding `ffmpeg` (or `ffmpeg.exe`) file inside the `YoutubeDownloader/` project directory.
You can also automatically download FFmpeg for your current platform by running the `DownloadFFmpeg.ps1` PowerShell script.
