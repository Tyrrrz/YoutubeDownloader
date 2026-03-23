#!/usr/bin/dotnet --
#:package CliFx

using System.IO.Compression;
using System.Runtime.InteropServices;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

return await new CliApplicationBuilder().AddCommand<DownloadFFmpegCommand>().Build().RunAsync(args);

[Command(Description = "Downloads FFmpeg.")]
public class DownloadFFmpegCommand : ICommand
{
    [CommandOption("output", Description = "Output path for the downloaded FFmpeg binary.")]
    public string OutputPath { get; init; } = Directory.GetCurrentDirectory();

    [CommandOption("platform", Description = "Target platform identifier (e.g. 'windows-x64').")]
    public string? Platform { get; init; }

    [CommandOption("ffmpeg-version", Description = "FFmpeg version to download.")]
    public string FFmpegVersion { get; init; } = "8.0.1";

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var platform = Platform;

        // If the platform is not specified, use the current OS/arch
        if (string.IsNullOrWhiteSpace(platform))
        {
            var arch = RuntimeInformation.OSArchitecture.ToString().ToLower();

            if (OperatingSystem.IsWindows())
                platform = $"windows-{arch}";
            else if (OperatingSystem.IsLinux())
                platform = $"linux-{arch}";
            else if (OperatingSystem.IsMacOS())
                platform = $"osx-{arch}";
            else
                throw new CommandException("Unsupported platform.");
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
        var archiveFilePath = outputPath + ".zip";
        var cancellationToken = console.RegisterCancellationHandler();
        try
        {
            using var http = new HttpClient();
            using var responseStream = await http.GetStreamAsync(
                $"https://github.com/Tyrrrz/FFmpegBin/releases/download/{FFmpegVersion}/ffmpeg-{platform}.zip",
                cancellationToken
            );

            await using (var archiveFile = File.Create(archiveFilePath))
            {
                await responseStream.CopyToAsync(archiveFile, cancellationToken);
            }

            // Extract FFmpeg
            using var zip = ZipFile.OpenRead(archiveFilePath);

            var entry =
                zip.GetEntry(fileName)
                ?? throw new CommandException(
                    $"Entry '{fileName}' not found in the downloaded archive."
                );

            entry.ExtractToFile(outputPath, true);

            console.Output.WriteLine("Done downloading FFmpeg.");
        }
        finally
        {
            // Clean up
            File.Delete(archiveFilePath);
        }
    }
}
