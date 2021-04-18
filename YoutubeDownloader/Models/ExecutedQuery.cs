using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Models
{
    public class ExecutedQuery
    {
        public Query Query { get; }

        public string Title { get; }

        public IReadOnlyList<IVideo> Videos { get; }

        public ExecutedQuery(Query query, string title, IReadOnlyList<IVideo> videos)
        {
            Query = query;
            Title = title;
            Videos = videos;
        }
    }
}