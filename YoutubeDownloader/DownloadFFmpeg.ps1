param (
    [string]$platform
)

$ErrorActionPreference = "Stop"

# Normalize platform identifier
$platform = $platform.ToLower().Replace("win-", "windows-")

# Determine output path
$ffmpegFileName = If ($platform.Contains("windows-")) { "ffmpeg.exe" } Else { "ffmpeg" }
$ffmpegFilePath = Join-Path $PSScriptRoot $ffmpegFileName

# Check if already exists
if (Test-Path $ffmpegFilePath) {
    Write-Host "Skipped downloading FFmpeg, file already exists."
    exit
}

# Download the archive
Write-Host "Downloading FFmpeg..."
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$http = New-Object System.Net.WebClient
try {
    $http.DownloadFile("https://github.com/Tyrrrz/FFmpegBin/releases/download/6.1.1/ffmpeg-$platform.zip", "$ffmpegFilePath.zip")
} finally {
    $http.Dispose()
}

try {
    # Extract FFmpeg
    Add-Type -Assembly System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead("$ffmpegFilePath.zip")
    try {
        [IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry($ffmpegFileName), $ffmpegFilePath)
    } finally {
        $zip.Dispose()
    }

    Write-Host "Done downloading FFmpeg."
} finally {
    # Clean up
    Remove-Item "$ffmpegFilePath.zip" -Force
}