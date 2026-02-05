using System;
using TagLib;
using TagFile = TagLib.File;

namespace YoutubeDownloader.Core.Tagging;

internal partial class MediaFile(TagFile file) : IDisposable
{
    public void SetThumbnail(byte[] thumbnailData) =>
        file.Tag.Pictures = [new Picture(thumbnailData)];

    public void SetArtist(string artist) => file.Tag.Performers = [artist];

    public void SetArtistSort(string artistSort) => file.Tag.PerformersSort = [artistSort];

    public void SetTitle(string title) => file.Tag.Title = title;

    public void SetAlbum(string album) => file.Tag.Album = album;

    public void SetDescription(string description) => file.Tag.Description = description;

    public void SetComment(string comment) => file.Tag.Comment = comment;

    public void Save()
    {
        file.Tag.DateTagged = DateTime.Now;
        file.Save();
    }

    public void Dispose() => file.Dispose();
}

internal partial class MediaFile
{
    public static MediaFile Open(string filePath)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        // Canonicalize the path to resolve any relative path components and prevent path traversal
        var fullPath = System.IO.Path.GetFullPath(filePath);

        // Verify the file exists to prevent information disclosure
        if (!System.IO.File.Exists(fullPath))
            throw new System.IO.FileNotFoundException("The specified file does not exist.", fullPath);

        return new(TagFile.Create(fullPath));
    }
}
