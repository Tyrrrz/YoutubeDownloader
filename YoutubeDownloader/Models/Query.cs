namespace YoutubeDownloader.Models
{
    public class Query
    {
        public QueryKind Kind { get; }

        public string Value { get; }

        public Query(QueryKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }
    }
}