using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchContext
    {
        private static AzureSearchContext instance;

        private AzureSearchContext() { }

        public static AzureSearchContext Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureSearchContext();
                }
                return instance;
            }
        }

        public IAzureSearchClient SearchClient { get; set; }
        public IAzureSearchIndexClient SearchIndexClient { get; set; }
    }
}
