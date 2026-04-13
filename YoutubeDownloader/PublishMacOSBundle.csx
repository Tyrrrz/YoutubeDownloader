#!/usr/bin/dotnet --
#:package CliFx

using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

[Command(Description = "Publishes the GUI app as a macOS .app bundle.")]
public partial class PublishMacOSBundleCommand : ICommand
{
    private const string BundleName = "YoutubeDownloader.app";
    private const string AppName = "YoutubeDownloader";
    private const string AppCopyright = "© Oleksii Holub";
    private const string AppIdentifier = "me.Tyrrrz.YoutubeDownloader";
    private const string AppSpokenName = "YoutubeDownloader";
    private const string AppIconName = "AppIcon";

    [CommandOption("publish-dir", Description = "Path to the publish output directory.")]
    public required string PublishDirPath { get; set; }

    [CommandOption("icons-file", Description = "Path to the .icns icons file.")]
    public required string IconsFilePath { get; set; }

    [CommandOption("full-version", Description = "Full version string (e.g. '1.2.3.4').")]
    public required string FullVersion { get; set; }

    [CommandOption("short-version", Description = "Short version string (e.g. '1.2.3').")]
    public required string ShortVersion { get; set; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        // Set up paths
        var publishDirPath = Path.GetFullPath(PublishDirPath);
        var tempDirPath = Path.GetFullPath(
            Path.Combine(publishDirPath, "../publish-macos-app-temp")
        );

        // Ensure the temporary directory is clean before use in case a previous run crashed
        if (Directory.Exists(tempDirPath))
            Directory.Delete(tempDirPath, true);

        var bundleDirPath = Path.Combine(tempDirPath, BundleName);
        var contentsDirPath = Path.Combine(bundleDirPath, "Contents");

        try
        {
            // Copy icons into the .app's Resources folder
            Directory.CreateDirectory(Path.Combine(contentsDirPath, "Resources"));
            File.Copy(
                IconsFilePath,
                Path.Combine(contentsDirPath, "Resources", "AppIcon.icns"),
                true
            );

            // Generate the Info.plist metadata file with the app information
            // lang=xml
            var plistContent = $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                <plist version="1.0">
                  <dict>
                    <key>CFBundleDisplayName</key>
                    <string>{AppName}</string>
                    <key>CFBundleName</key>
                    <string>{AppName}</string>
                    <key>CFBundleExecutable</key>
                    <string>{AppName}</string>
                    <key>NSHumanReadableCopyright</key>
                    <string>{AppCopyright}</string>
                    <key>CFBundleIdentifier</key>
                    <string>{AppIdentifier}</string>
                    <key>CFBundleSpokenName</key>
                    <string>{AppSpokenName}</string>
                    <key>CFBundleIconFile</key>
                    <string>{AppIconName}</string>
                    <key>CFBundleIconName</key>
                    <string>{AppIconName}</string>
                    <key>CFBundleVersion</key>
                    <string>{FullVersion}</string>
                    <key>CFBundleShortVersionString</key>
                    <string>{ShortVersion}</string>
                    <key>NSHighResolutionCapable</key>
                    <true />
                    <key>CFBundlePackageType</key>
                    <string>APPL</string>
                  </dict>
                </plist>
                """;

            await File.WriteAllTextAsync(Path.Combine(contentsDirPath, "Info.plist"), plistContent);

            // Delete the previous bundle if it exists
            var existingBundlePath = Path.Combine(publishDirPath, BundleName);
            if (Directory.Exists(existingBundlePath))
                Directory.Delete(existingBundlePath, true);

            // Move all files from the publish directory into the MacOS directory
            Directory.CreateDirectory(Path.Combine(contentsDirPath, "MacOS"));
            foreach (var entryPath in Directory.GetFileSystemEntries(publishDirPath))
            {
                var destinationPath = Path.Combine(
                    contentsDirPath,
                    "MacOS",
                    Path.GetFileName(entryPath)
                );

                if (Directory.Exists(entryPath))
                    Directory.Move(entryPath, destinationPath);
                else
                    File.Move(entryPath, destinationPath);
            }

            // Move the final bundle into the publish directory for upload
            Directory.Move(bundleDirPath, Path.Combine(publishDirPath, BundleName));
        }
        finally
        {
            // Clean up the temporary directory
            if (Directory.Exists(tempDirPath))
                Directory.Delete(tempDirPath, true);
        }
    }
}
