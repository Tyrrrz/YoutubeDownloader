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

    public void Dispose()
    {
        file.Tag.DateTagged = DateTime.Now;
        file.Save();
        file.Dispose();
    }
}

internal partial class MediaFile
{
    public static MediaFile Create(string filePath) => new(TagFile.Create(filePath));
}
