param (
    [string]$platform,
    [string]$outputPath
)

$ErrorActionPreference = "Stop"

# If the platform is not specified, use the current OS/arch
if (-not $platform) {
    $arch = [Runtime.InteropServices.RuntimeInformation]::OSArchitecture

    if ($isWindows) {
        $platform = "windows-$arch"
    } elseif ($isLinux) {
        $platform = "linux-$arch"
    } elseif ($isMacOS) {
        $platform = "osx-$arch"
    } else {
        throw "Unsupported platform"
    }
}

# Normalize platform identifier
$platform = $platform.ToLower().Replace("win-", "windows-")

# If the output path is not specified, use the current directory
if (-not $outputPath) {
    $fileName = if ($platform.Contains("windows-")) { "ffmpeg.exe" } else { "ffmpeg" }
    $outputPath = "$PSScriptRoot/$fileName"
}

# Delete the existing file if it exists
if (Test-Path $outputPath) {
    Remove-Item $outputPath
}

# Download the archive
Write-Host "Downloading FFmpeg for $platform..."
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$http = New-Object System.Net.WebClient
try {
    $http.DownloadFile("https://github.com/Tyrrrz/FFmpegBin/releases/download/7.0/ffmpeg-$platform.zip", "$outputPath.zip")
} finally {
    $http.Dispose()
}

try {
    # Extract FFmpeg
    Add-Type -Assembly System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead("$outputPath.zip")
    try {
        $fileName = If ($platform.Contains("windows-")) { "ffmpeg.exe" } Else { "ffmpeg" }
        [IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry($fileName), $outputPath)
    } finally {
        $zip.Dispose()
    }

    Write-Host "Done downloading FFmpeg."
} finally {
    # Clean up
    Remove-Item "$outputPath.zip" -Force
}