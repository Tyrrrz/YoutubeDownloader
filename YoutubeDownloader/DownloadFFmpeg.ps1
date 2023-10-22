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
    $http.DownloadFile("https://github.com/GyanD/codexffmpeg/releases/download/6.0/ffmpeg-6.0-essentials_build.zip", "$ffmpegFilePath.zip")
} finally {
    $http.Dispose()
}

try {
    Import-Module "$PSHOME/Modules/Microsoft.PowerShell.Utility" -Function Get-FileHash
    $hashResult = Get-FileHash "$ffmpegFilePath.zip" -Algorithm SHA256
    if ($hashResult.Hash -ne "9ae315c4310bda9127ff1ef7ea8cbe968c2c2fb70287c71a3d9c2251d629289e") {
        throw "Failed to verify the hash of the FFmpeg archive."
    }

    # Extract FFmpeg
    Add-Type -Assembly System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead("$ffmpegFilePath.zip")
    try {
        [IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry("ffmpeg-6.0-essentials_build/bin/ffmpeg.exe"), $ffmpegFilePath)
    } finally {
        $zip.Dispose()
    }

    Write-Host "Done downloading FFmpeg."
} finally {
    # Clean up
    Remove-Item "$ffmpegFilePath.zip" -Force
}