param(
    [Parameter(Mandatory=$true)]
    [string]$PublishDirPath,

    [Parameter(Mandatory=$true)]
    [string]$IconsFilePath,

    [Parameter(Mandatory=$true)]
    [string]$FullVersion,

    [Parameter(Mandatory=$true)]
    [string]$ShortVersion
)

$ErrorActionPreference = "Stop"

# Setup paths
$tempDirPath = Join-Path $PublishDirPath "../publish-macos-app-temp"
$bundleName = "YoutubeDownloader.app"
$bundleDirPath = Join-Path $tempDirPath $bundleName
$contentsDirPath = Join-Path $bundleDirPath "Contents"
$macosDirPath = Join-Path $contentsDirPath "MacOS"
$resourcesDirPath = Join-Path $contentsDirPath "Resources"

try {
    # Initialize the bundle's directory structure
    New-Item -Path $bundleDirPath -ItemType Directory -Force
    New-Item -Path $contentsDirPath -ItemType Directory -Force
    New-Item -Path $macosDirPath -ItemType Directory -Force
    New-Item -Path $resourcesDirPath -ItemType Directory -Force

    # Copy icons into the .app's Resources folder
    Copy-Item -Path $IconsFilePath -Destination (Join-Path $resourcesDirPath "AppIcon.icns") -Force

    # Generate the Info.plist metadata file with the app information
    $plistContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>CFBundleDisplayName</key>
    <string>YoutubeDownloader</string>
    <key>CFBundleName</key>
    <string>YoutubeDownloader</string>
    <key>CFBundleExecutable</key>
    <string>YoutubeDownloader</string>
    <key>NSHumanReadableCopyright</key>
    <string>© Oleksii Holub</string>
    <key>CFBundleIdentifier</key>
    <string>me.Tyrrrz.YoutubeDownloader</string>
    <key>CFBundleSpokenName</key>
    <string>YoutubeDownloader</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>CFBundleIconName</key>
    <string>AppIcon</string>
    <key>CFBundleVersion</key>
    <string>$FullVersion</string>
    <key>CFBundleShortVersionString</key>
    <string>$ShortVersion</string>
    <key>NSHighResolutionCapable</key>
    <true />
    <key>CFBundlePackageType</key>
    <string>APPL</string>
  </dict>
</plist>
"@

    Set-Content -Path (Join-Path $contentsDirPath "Info.plist") -Value $plistContent

    # Delete the previous bundle if it exists
    if (Test-Path (Join-Path $PublishDirPath $bundleName)) {
        Remove-Item -Path (Join-Path $PublishDirPath $bundleName) -Recurse -Force
    }

    # Move all files from the publish directory into the MacOS directory
    Get-ChildItem -Path $PublishDirPath | ForEach-Object {
        Move-Item -Path $_.FullName -Destination $macosDirPath -Force
    }

    # Move the final bundle into the publish directory for upload
    Move-Item -Path $bundleDirPath -Destination $PublishDirPath -Force
}
finally {
    # Clean up the temporary directory
    Remove-Item -Path $tempDirPath -Recurse -Force
}