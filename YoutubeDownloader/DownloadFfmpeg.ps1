$ffmpegFilePath = "$PSScriptRoot\ffmpeg.exe"

# Check if already exists
if (Test-Path $ffmpegFilePath) {
    Write-Host "Skipped downloading ffmpeg, file already exists."
    exit
}

Write-Host "Downloading ffmpeg..."

# Download the zip archive
$url = "https://github.com/vot/ffbinaries-prebuilt/releases/download/v4.1/ffmpeg-4.1-win-64.zip"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$wc = New-Object System.Net.WebClient
$wc.DownloadFile($url, "$ffmpegFilePath.zip")
$wc.Dispose()

# Extract ffmpeg.exe from the archive
Add-Type -Assembly System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead("$ffmpegFilePath.zip")
[IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry("ffmpeg.exe"), $ffmpegFilePath)
$zip.Dispose()

# Delete the archive
Remove-Item "$ffmpegFilePath.zip" -Force

Write-Host "Done downloading ffmpeg."