namespace YoutubeDownloader.Models
{
    public class Query
    {
        public QueryType Type { get; }

        public string Value { get; }

        public Query(QueryType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}