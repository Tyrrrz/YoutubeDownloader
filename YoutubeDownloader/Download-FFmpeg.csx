using System.IO.Compression;
using System.Runtime.InteropServices;

string? platform = null;
var outputPath = Directory.GetCurrentDirectory();

for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--platform" && i + 1 < args.Length)
        platform = args[++i];
    else if (args[i] == "--output-path" && i + 1 < args.Length)
        outputPath = args[++i];
}

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

// If the output path is an existing directory, append the default file name for the platform
if (Directory.Exists(outputPath))
    outputPath = Path.Combine(outputPath, fileName);

// Delete the existing file if it exists
if (File.Exists(outputPath))
    File.Delete(outputPath);

// Download the archive
Console.WriteLine($"Downloading FFmpeg for {platform}...");
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

    Console.WriteLine("Done downloading FFmpeg.");
}
finally
{
    // Clean up
    File.Delete(archivePath);
}
