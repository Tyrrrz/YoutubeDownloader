$ErrorActionPreference = 'Stop';
$packageArgs = @{
  packageName = $env:ChocolateyPackageName
  unzipLocation = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
  url = 'https://github.com/Tyrrrz/YoutubeDownloader/releases/download/1.3.1/YoutubeDownloader.zip'
}
Install-ChocolateyZipPackage @packageArgs