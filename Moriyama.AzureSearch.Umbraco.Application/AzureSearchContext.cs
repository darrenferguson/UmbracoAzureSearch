using log4net;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using System.Reflection;
using Moriyama.AzureSearch.Umbraco.Application.Helper;
using Moriyama.AzureSearch.Umbraco.Application.Models;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchContext
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static AzureSearchContext _instance;

        private IAzureSearchIndexClient _searchIndexClient { get; set; }

        private IAzureSearchClient _searchClient { get; set; }

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

        public IAzureSearchIndexClient SearchIndexClient
        {
            get
            {
                return this._searchIndexClient;
            }
        }

        public IAzureSearchClient SearchClient
        {
            get
            {                
                return this._searchClient;
            }
        }

        public void Initialise(AzureSearchConfig configuration, string tempDirectory)
        {
            IUmbracoDependencyHelper umbracoDependencyHelper = new UmbracoDependencyHelper();
            this._searchClient = new AzureSearchClient(configuration);
            this._searchIndexClient = new AzureSearchIndexClient(configuration, tempDirectory, umbracoDependencyHelper);

            Log.Info("AzureSearchContext initialised.");
        }
    }
}
