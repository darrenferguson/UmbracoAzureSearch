using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using System;
using System.Configuration;
using System.Web;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchContext
    {
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

                    if (_instance.AutomaticStartUp)
                    {
                        var appRoot = ConfigurationManager.AppSettings["Moriyama.AzureSearch.AppRoot"] ?? "/";
                        _instance.StartUp(HttpContext.Current.Server.MapPath(appRoot));
                    }
                }
                return _instance;
            }
        }

        public IAzureSearchIndexClient SearchIndexClient
        {
            get
            {
                if (_searchIndexClient == null && !AutomaticStartUp)
                {
                    throw new NullReferenceException("Automatic start up is disabled; make sure to call StartUp(appRoot) explicitly.");
                }

                return _searchIndexClient;
            }
        }

        public IAzureSearchClient SearchClient
        {
            get
            {
                if (_searchClient == null && !AutomaticStartUp)
                {
                    throw new NullReferenceException("Automatic start up is disabled; make sure to call StartUp(appRoot) explicitly.");
                }

                return _searchClient;
            }
        }

        public void StartUp(string appRoot)
        {
            _searchClient = new AzureSearchClient(appRoot);
            _searchIndexClient = new AzureSearchIndexClient(appRoot);
        }

        public bool AutomaticStartUp
        {
            get
            {
                var disabledStartUp = ConfigurationManager.AppSettings["Moriyama.AzureSearch.DisabledStartUp"] ?? "False";
                return !bool.Parse(disabledStartUp);
            }
        }
    }
}
