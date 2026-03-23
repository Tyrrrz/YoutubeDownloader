string? publishDirPath = null;
string? iconsFilePath = null;
string? fullVersion = null;
string? shortVersion = null;

for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--publish-dir-path" && i + 1 < args.Length)
        publishDirPath = args[++i];
    else if (args[i] == "--icons-file-path" && i + 1 < args.Length)
        iconsFilePath = args[++i];
    else if (args[i] == "--full-version" && i + 1 < args.Length)
        fullVersion = args[++i];
    else if (args[i] == "--short-version" && i + 1 < args.Length)
        shortVersion = args[++i];
}

if (publishDirPath is null)
    throw new Exception("--publish-dir-path is required");
if (iconsFilePath is null)
    throw new Exception("--icons-file-path is required");
if (fullVersion is null)
    throw new Exception("--full-version is required");
if (shortVersion is null)
    throw new Exception("--short-version is required");

// Setup paths
var tempDirPath = Path.GetFullPath(Path.Combine(publishDirPath, "../publish-macos-app-temp"));
var bundleName = "YoutubeDownloader.app";
var bundleDirPath = Path.Combine(tempDirPath, bundleName);
var contentsDirPath = Path.Combine(bundleDirPath, "Contents");
var macosDirPath = Path.Combine(contentsDirPath, "MacOS");
var resourcesDirPath = Path.Combine(contentsDirPath, "Resources");

try
{
    // Initialize the bundle's directory structure
    Directory.CreateDirectory(bundleDirPath);
    Directory.CreateDirectory(contentsDirPath);
    Directory.CreateDirectory(macosDirPath);
    Directory.CreateDirectory(resourcesDirPath);

    // Copy icons into the .app's Resources folder
    File.Copy(iconsFilePath, Path.Combine(resourcesDirPath, "AppIcon.icns"), overwrite: true);

    // Generate the Info.plist metadata file with the app information
    var plistContent = $"""
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
            <string>{fullVersion}</string>
            <key>CFBundleShortVersionString</key>
            <string>{shortVersion}</string>
            <key>NSHighResolutionCapable</key>
            <true />
            <key>CFBundlePackageType</key>
            <string>APPL</string>
          </dict>
        </plist>
        """;

    File.WriteAllText(Path.Combine(contentsDirPath, "Info.plist"), plistContent);

    // Delete the previous bundle if it exists
    var existingBundlePath = Path.Combine(publishDirPath, bundleName);
    if (Directory.Exists(existingBundlePath))
        Directory.Delete(existingBundlePath, recursive: true);

    // Move all files from the publish directory into the MacOS directory
    foreach (var entry in Directory.GetFileSystemEntries(publishDirPath))
    {
        var destination = Path.Combine(macosDirPath, Path.GetFileName(entry));
        if (File.Exists(entry))
            File.Move(entry, destination, overwrite: true);
        else if (Directory.Exists(entry))
            Directory.Move(entry, destination);
    }

    // Move the final bundle into the publish directory for upload
    Directory.Move(bundleDirPath, Path.Combine(publishDirPath, bundleName));
}
finally
{
    // Clean up the temporary directory
    if (Directory.Exists(tempDirPath))
        Directory.Delete(tempDirPath, recursive: true);
}
