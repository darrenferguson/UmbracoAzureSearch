using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using System.IO;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public abstract class BaseAzureSearch
    {
        protected AzureSearchConfig _config;
        protected readonly string _path;
        private ISearchServiceClient _searchServiceClient;

        public BaseAzureSearch(string path) : this(path, null)
        {
        }

        public BaseAzureSearch(string path, ISearchServiceClient searchServiceClient)
        {
            _path = path;
            _config = JsonConvert.DeserializeObject<AzureSearchConfig>(File.ReadAllText(Path.Combine(_path, @"config\AzureSearch.config")));
            _searchServiceClient = searchServiceClient == null
                                    ? new SearchServiceClient(_config.SearchServiceName, new SearchCredentials(_config.SearchServiceAdminApiKey))
                                    : _searchServiceClient = searchServiceClient;
        }

        public void SaveConfiguration(AzureSearchConfig config)
        {
            _config = config;
            File.WriteAllText(Path.Combine(_path, @"config\AzureSearch.config"), JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public AzureSearchConfig GetConfiguration()
        {
            return _config;
        }

        public ISearchServiceClient GetClient()
        {
            return _searchServiceClient;
        }
    }
}
