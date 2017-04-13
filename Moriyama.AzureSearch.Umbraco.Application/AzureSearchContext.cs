using System;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchContext
    {
        private static AzureSearchContext _instance;
        private object[] _args;
        private Type _azureSearchClientType;

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

        public IAzureSearchIndexClient SearchIndexClient { get; set; }

        public void SetupSearchClient<T>(params object[] args) where T : IAzureSearchClient
        {
            _args = args;
            _azureSearchClientType = typeof(T);
        }

        public IAzureSearchClient GetSearchClient()
        {
            if (_azureSearchClientType == null)
            {
                throw new ArgumentException("_azureSearchClientType has not been set");
            }

            return (IAzureSearchClient)Activator.CreateInstance(_azureSearchClientType, _args);
        }
    }
}
