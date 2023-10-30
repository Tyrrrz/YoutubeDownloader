$ErrorActionPreference = "Stop"
$ffmpegFilePath = "$PSScriptRoot/ffmpeg.exe"

# Check if already exists
if (Test-Path $ffmpegFilePath) {
    Write-Host "Skipped downloading FFmpeg, file already exists."
    exit
}

Write-Host "Downloading FFmpeg..."

# Download the archive
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$http = New-Object System.Net.WebClient
try {
    $http.DownloadFile("https://github.com/Tyrrrz/FFmpegBin/releases/download/6.0/ffmpeg-windows-x64.zip", "$ffmpegFilePath.zip")
} finally {
    $http.Dispose()
}

try {
    Import-Module "$PSHOME/Modules/Microsoft.PowerShell.Utility" -Function Get-FileHash
    $hashResult = Get-FileHash "$ffmpegFilePath.zip" -Algorithm SHA256
    if ($hashResult.Hash -ne "29289b1008a8fadbb012e7dc0e325fea9eebbe87ac2019a4fa7df7fc15af02d0") {
        throw "Failed to verify the hash of the FFmpeg archive."
    }

    # Extract FFmpeg
    Add-Type -Assembly System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead("$ffmpegFilePath.zip")
    try {
        [IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry("ffmpeg.exe"), $ffmpegFilePath)
    } finally {
        $zip.Dispose()
    }

    Write-Host "Done downloading FFmpeg."
} finally {
    # Clean up
    Remove-Item "$ffmpegFilePath.zip" -Force
}