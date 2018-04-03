using Microsoft.Azure.Search.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class AzureSearchConfig
    {
        public string SearchServiceName { get; set; }

        public string SearchServiceAdminApiKey { get; set; }

        public string IndexName { get; set; }

        public int IndexBatchSize { get; set; }

        public SearchField[] Fields { get; set; }

        public Analyzer[] Analyzers { get; set; }

        public ScoringProfile[] ScoringProfiles { get; set; }

        public Suggester[] Suggesters { get; set; }

    }
}
