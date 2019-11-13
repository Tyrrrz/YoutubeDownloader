using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeDownloader.Models;

namespace YoutubeDownloader.Services
{
    public static class Extensions
    {
        public static IReadOnlyList<Query> ParseMultilineQuery(this QueryService queryService, string query) =>
            query.Split(Environment.NewLine).Select(queryService.ParseQuery).ToArray();

        public static async Task<IReadOnlyList<ExecutedQuery>> ExecuteQueriesAsync(this QueryService queryService, IReadOnlyList<Query> queries,
            IProgress<double>? progress = null)
        {
            var result = new List<ExecutedQuery>(queries.Count);

            for (var i = 0; i < queries.Count; i++)
            {
                var executedQuery = await queryService.ExecuteQueryAsync(queries[i]);
                result.Add(executedQuery);

                progress?.Report((i + 1.0) / queries.Count);
            }

            return result;
        }
    }
}