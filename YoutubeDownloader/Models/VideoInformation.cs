using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Models
{
    public class VideoInformation
    {
        private VideoInformation(VideoId id, string title, string author, DateTimeOffset? uploadDate, TimeSpan duration,
            ThumbnailSet thumbnails)
        {
            Id = id;
            Title = title;
            Author = author;
            UploadDate = uploadDate;
            Duration = duration;
            Thumbnails = thumbnails;
        }

        /// <summary>
        ///     Video ID.
        /// </summary>
        public VideoId Id { get; }

        /// <summary>
        ///     Video title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Video author.
        /// </summary>
        public string Author { get; }

        /// <summary>
        ///     Video upload date.
        /// </summary>
        public DateTimeOffset? UploadDate { get; }

        /// <summary>
        ///     Duration of the video.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        ///     Available thumbnails for this video.
        /// </summary>
        public ThumbnailSet Thumbnails { get; }

        public static IReadOnlyList<VideoInformation> VideoInformationAsList(Video video)
        {
            return new[]
            {
                new VideoInformation(video.Id, video.Title, video.Author, video.UploadDate, video.Duration,
                    video.Thumbnails)
            };
        }

        public static IReadOnlyList<VideoInformation> VideoInformationAsList(IEnumerable<PlaylistVideo> videos)
        {
            return videos.Select(video => new VideoInformation(video.Id, video.Title, video.Author, null, 
                video.Duration, video.Thumbnails)).ToList();
        }
    }
}