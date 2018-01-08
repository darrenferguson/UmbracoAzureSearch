using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchContext
    {
        private static AzureSearchContext _instance;

        private AzureSearchContext() { }

        public static AzureSearchContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AzureSearchContext();
                }
                return _instance;
            }
        }

        public IAzureSearchIndexClient SearchIndexClient { get; internal set; }

        public IAzureSearchClient SearchClient { get; internal set; }
    }
}
