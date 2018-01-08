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
        private const string ConfigRelativePath = @"config\AzureSearch.config";

        public BaseAzureSearch(string path)
        {
            _path = path;
            _config = JsonConvert.DeserializeObject<AzureSearchConfig>(File.ReadAllText(GetConfigFullPath()));
            _searchServiceClient = new SearchServiceClient(_config.SearchServiceName, new SearchCredentials(_config.SearchServiceAdminApiKey));
        }

        public void SaveConfiguration(AzureSearchConfig config)
        {
            _config = config;
            File.WriteAllText(GetConfigFullPath(), JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public AzureSearchConfig GetConfiguration()
        {
            return _config;
        }

        public ISearchServiceClient GetClient()
        {
            return _searchServiceClient;
        }

        private string GetConfigFullPath()
        {
            return Path.Combine(_path, ConfigRelativePath);
        }
    }
}
