using System.IO;
using Java.Lang;
using YoutubeDownloader.Core.Downloading;

namespace YoutubeDownloader.Android;

/// <summary>
/// Points the shared FFmpeg resolver at the binary bundled inside the APK.
/// </summary>
/// <remarks>
/// Android forbids executing binaries from app data, but files in the native library
/// directory are executable. FFmpeg is therefore packaged as <c>libffmpeg.so</c> (see the
/// AndroidNativeLibrary items in the csproj) even though it is a CLI executable rather than
/// a shared library, and is run from there. This requires <c>extractNativeLibs="true"</c> in
/// the manifest, otherwise the entry stays mapped inside the APK and is not a real file.
/// </remarks>
public static class AndroidFFmpegInitializer
{
    public static void Initialize()
    {
        var applicationInfo = global::Android.App.Application.Context.ApplicationInfo;
        if (applicationInfo?.NativeLibraryDir is not { } nativeLibraryDir)
            return;

        var filePath = Path.Combine(nativeLibraryDir, "libffmpeg.so");
        if (!File.Exists(filePath))
            return;

        // Force the loader to map the library, which also surfaces an ABI mismatch here
        // rather than as an opaque failure on the first download.
        JavaSystem.LoadLibrary("ffmpeg");

        FFmpeg.CustomCliFilePath = filePath;
    }
}
