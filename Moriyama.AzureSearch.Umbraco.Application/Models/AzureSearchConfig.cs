using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class AzureSearchConfig
    {
        public string SearchServiceName { get; set; }

        public string SearchServiceAdminApiKey { get; set; }

        public string IndexName { get; set; }

        public SearchField[] SearchFields { get; set; }

		public bool LogSearchPerformance { get; set; }

        public string DefaultScoringProfile { get; set; }

        public List<AzureSearchScoringProfile> ScoringProfiles { get; set; }

        public int ReIndexBatchSize { get; set; }
	}
}
