using log4net;
using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using System.Reflection;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public abstract class BaseAzureSearch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected AzureSearchConfig _configuration;
       
        private readonly ISearchServiceClient _searchServiceClient;
        
        public BaseAzureSearch(AzureSearchConfig configuration)
        {
            this._configuration = configuration;
            this._searchServiceClient = new SearchServiceClient(_configuration.SearchServiceName, new SearchCredentials(_configuration.SearchServiceAdminApiKey));
        }
        
        public AzureSearchConfig GetConfiguration()
        {
            return this._configuration;
        }

        public ISearchServiceClient GetClient()
        {
            return this._searchServiceClient;
        }
    }
}
