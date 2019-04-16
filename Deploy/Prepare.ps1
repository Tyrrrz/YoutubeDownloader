# Get files
$files = @()
$files += Get-Item -Path "$PSScriptRoot\..\License.txt"
$files += Get-ChildItem -Path "$PSScriptRoot\..\YoutubeDownloader\bin\Release\*" -Include "*.exe", "*.dll", "*.config"

# Pack into archive
New-Item "$PSScriptRoot\Portable\bin" -ItemType Directory -Force
$files | Compress-Archive -DestinationPath "$PSScriptRoot\Portable\bin\YoutubeDownloader.zip" -Force