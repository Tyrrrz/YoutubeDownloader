using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.Framework;

public class DialogManager : IDisposable
{
    private readonly SemaphoreSlim _dialogLock = new(1, 1);

    /// <summary>
    /// Turns an Android storage-picker URI into a real file system path, or null if it
    /// does not describe one.
    /// </summary>
    /// <remarks>
    /// Android's picker hands back a Storage Access Framework URI rather than a path, e.g.
    /// <c>content://com.android.externalstorage.documents/tree/primary%3ADownload</c>, and
    /// <c>TryGetLocalPath()</c> returns null for it. Passing that string
    /// on as if it were a path produced "Read-only file system : '/content:'" the moment
    /// anything tried to create the directory.
    ///
    /// Locations on the primary volume map onto a real path, which the app can use because
    /// it holds all-files access. That matters beyond convenience: downloads are muxed by
    /// the FFmpeg binary, which takes a path and cannot write into a content URI.
    ///
    /// Anything else (SD cards, cloud providers) has no such mapping and returns null, so
    /// the caller can reject it rather than fail later with an opaque IO error.
    ///
    /// Pure string handling, so it needs no Android-specific API.
    /// </remarks>
    internal static string? TryResolveAndroidStorageUri(string uri)
    {
        const string treeMarker = "/tree/";

        var markerIndex = uri.IndexOf(treeMarker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return null;

        // "primary%3ADownload/Videos" -> volume "primary", relative path "Download/Videos"
        var spec = Uri.UnescapeDataString(uri[(markerIndex + treeMarker.Length)..]);

        var separatorIndex = spec.IndexOf(':');
        if (separatorIndex < 0)
            return null;

        var volume = spec[..separatorIndex];
        var relativePath = spec[(separatorIndex + 1)..].Trim('/');

        if (!string.Equals(volume, "primary", StringComparison.OrdinalIgnoreCase))
            return null;

        // The primary volume is always the current user's shared storage.
        return string.IsNullOrEmpty(relativePath)
            ? "/storage/emulated/0"
            : $"/storage/emulated/0/{relativePath}";
    }

    // Picked locations arrive as a local path on desktop and as a SAF URI on Android.
    private static string? ToLocalPath(IStorageItem item)
    {
        if (item.TryGetLocalPath() is { } localPath && !string.IsNullOrWhiteSpace(localPath))
            return localPath;

        var uri = item.Path.ToString();

        if (OperatingSystem.IsAndroid())
            return TryResolveAndroidStorageUri(uri);

        return uri;
    }

    public async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialog)
    {
        await _dialogLock.WaitAsync();
        try
        {
            await DialogHost.Show(
                dialog,
                // It's fine to await in a void method here because it's an event handler
                // ReSharper disable once AsyncVoidLambda
                async (object _, DialogOpenedEventArgs args) =>
                {
                    await dialog.WaitForCloseAsync();

                    try
                    {
                        args.Session.Close();
                    }
                    catch (InvalidOperationException)
                    {
                        // Dialog host is already processing a close operation
                    }
                }
            );

            // Yield to allow DialogHost to fully reset its state before
            // another dialog is shown (e.g., when dialogs are shown sequentially)
            await Task.Yield();

            return dialog.DialogResult;
        }
        finally
        {
            _dialogLock.Release();
        }
    }

    public async Task<string?> PromptOpenFilePathAsync(
        IReadOnlyList<FilePickerFileType>? fileTypes = null
    )
    {
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        var result = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { FileTypeFilter = fileTypes, AllowMultiple = false }
        );

        var file = result.FirstOrDefault();
        return file is null ? null : ToLocalPath(file);
    }

    public async Task<string?> PromptSaveFilePathAsync(
        IReadOnlyList<FilePickerFileType>? fileTypes = null,
        string defaultFilePath = ""
    )
    {
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                FileTypeChoices = fileTypes,
                SuggestedFileName = defaultFilePath,
                DefaultExtension = Path.GetExtension(defaultFilePath).TrimStart('.'),
            }
        );

        return file is null ? null : ToLocalPath(file);
    }

    public async Task<string?> PromptDirectoryPathAsync(string defaultDirPath = "")
    {
        var topLevel =
            Application.Current?.ApplicationLifetime?.TryGetTopLevel()
            ?? throw new ApplicationException("Could not find the top-level visual element.");

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(
                    defaultDirPath
                ),
            }
        );

        var directory = result.FirstOrDefault();
        if (directory is null)
            return null;

        return ToLocalPath(directory);
    }

    public void Dispose() => _dialogLock.Dispose();
}
