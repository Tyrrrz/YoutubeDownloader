$ErrorActionPreference = 'Stop'
$packageName = $env:ChocolateyPackageName
$installDirPath = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# Install package
$packageArgs = @{
    packageName   = $packageName
    unzipLocation = $installDirPath
    url           = 'https://github.com/Tyrrrz/YoutubeDownloader/releases/download/1.3.1/YoutubeDownloader.zip'
}
Install-ChocolateyZipPackage @packageArgs

# Mark the executable as GUI
New-Item (Join-Path $installDirPath "YoutubeDownloader.exe.gui") -ItemType File -Force

# Don't put ffmpeg.exe on PATH
New-Item (Join-Path $installDirPath "ffmpeg.exe.ignore") -ItemType File -Force