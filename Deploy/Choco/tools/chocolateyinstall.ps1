$ErrorActionPreference = 'Stop';

# Install package
$packageArgs = @{
    packageName   = $env:ChocolateyPackageName
    unzipLocation = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
    url           = 'https://github.com/Tyrrrz/YoutubeDownloader/releases/download/1.3.1/YoutubeDownloader.zip'
}
Install-ChocolateyZipPackage @packageArgs

# Mark the executable as GUI
New-Item (Join-Path unzipLocation "YoutubeDownloader.exe.gui") -ItemType File -Force

# Don't put ffmpeg.exe on PATH
New-Item (Join-Path unzipLocation "ffmpeg.exe.ignore") -ItemType File -Force