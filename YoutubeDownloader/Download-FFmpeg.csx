#!/usr/bin/env -S dotnet run --
#:package CliFx

using System.IO.Compression;
using System.Runtime.InteropServices;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

return await new CliApplicationBuilder()
    .AddCommand<DownloadFFmpegCommand>()
    .Build()
    .RunAsync(args);

[Command(Description = "Downloads FFmpeg.")]
public class DownloadFFmpegCommand : ICommand
{
    [CommandOption("platform", Description = "Target platform identifier (e.g. 'windows-x64').")]
    public string? Platform { get; init; }

    [CommandOption("output-path", Description = "Output path for the downloaded FFmpeg binary.")]
    public string OutputPath { get; init; } = Directory.GetCurrentDirectory();

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var platform = Platform;

        // If the platform is not specified, use the current OS/arch
        if (platform is null)
        {
            var arch = RuntimeInformation.OSArchitecture.ToString().ToLower();

            if (OperatingSystem.IsWindows())
                platform = $"windows-{arch}";
            else if (OperatingSystem.IsLinux())
                platform = $"linux-{arch}";
            else if (OperatingSystem.IsMacOS())
                platform = $"osx-{arch}";
            else
                throw new Exception("Unsupported platform");
        }

        // Normalize platform identifier
        platform = platform.ToLower().Replace("win-", "windows-");

        // Identify the FFmpeg filename based on the platform
        var fileName = platform.Contains("windows-") ? "ffmpeg.exe" : "ffmpeg";

        var outputPath = OutputPath;

        // If the output path is an existing directory, append the default file name for the platform
        if (Directory.Exists(outputPath))
            outputPath = Path.Combine(outputPath, fileName);

        // Delete the existing file if it exists
        if (File.Exists(outputPath))
            File.Delete(outputPath);

        // Download the archive
        console.Output.WriteLine($"Downloading FFmpeg for {platform}...");
        using var http = new HttpClient();
        var archivePath = outputPath + ".zip";
        var archiveBytes = await http.GetByteArrayAsync(
            $"https://github.com/Tyrrrz/FFmpegBin/releases/download/7.1.2/ffmpeg-{platform}.zip"
        );
        await File.WriteAllBytesAsync(archivePath, archiveBytes);

        try
        {
            // Extract FFmpeg
            using var zip = ZipFile.OpenRead(archivePath);
            var entry =
                zip.GetEntry(fileName)
                ?? throw new Exception($"Entry '{fileName}' not found in the downloaded archive.");
            entry.ExtractToFile(outputPath, overwrite: true);

            console.Output.WriteLine("Done downloading FFmpeg.");
        }
        finally
        {
            // Clean up
            File.Delete(archivePath);
        }
    }
}
