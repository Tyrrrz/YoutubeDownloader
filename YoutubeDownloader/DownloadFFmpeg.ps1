$ffmpegFilePath = "$PSScriptRoot\ffmpeg.exe"

# Check if already exists
if (Test-Path $ffmpegFilePath) {
    Write-Host "Skipped downloading ffmpeg, file already exists."
    exit
}

Write-Host "Downloading ffmpeg..."

# Download the zip archive
#$url = "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.4.1/ffmpeg-4.4.1-win-64.zip"
$url = "https://github.com/sudo-nautilus/FFmpeg-Builds-Win32/releases/download/latest/ffmpeg-master-latest-win32-gpl.zip"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$wc = New-Object System.Net.WebClient
$wc.DownloadFile($url, "$ffmpegFilePath.zip")
$wc.Dispose()

# Extract ffmpeg.exe from the archive
Add-Type -Assembly System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead("$ffmpegFilePath.zip")
#[IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry("ffmpeg.exe"), $ffmpegFilePath)

$filesToExtract = @(
    "ffmpeg-master-latest-win32-gpl/bin/ffmpeg.exe";
    "ffmpeg.exe";
)

foreach($entry in $zip.Entries){
    Write-Host "Checking ==> " $entry.FullName
    if ($filesToExtract -contains $entry.FullName){
        Write-Host "Extract ==> " + $entry.FullName  " ==> "  $ffmpegFilePath
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $ffmpegFilePath)
        #$entry.ExtractToFile($ffmpegFilePath)
        break
    }
}
$zip.Dispose()

# Delete the archive
Remove-Item "$ffmpegFilePath.zip" -Force

Write-Host "Done downloading ffmpeg."