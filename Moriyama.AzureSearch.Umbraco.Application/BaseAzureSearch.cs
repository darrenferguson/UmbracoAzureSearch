using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using System.IO;
using StackExchange.Profiling;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public abstract class BaseAzureSearch
    {
        protected AzureSearchConfig _config;
        protected readonly string _path;

        // Number of docs to be processed at a time.
        const int BatchSize = 999;

        public BaseAzureSearch(string path)
        {
            _path = path;
            _config = JsonConvert.DeserializeObject<AzureSearchConfig>(File.ReadAllText(Path.Combine(path, @"config\AzureSearch.config")));
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

        public SearchServiceClient GetClient()
        {
            var profiler = MiniProfiler.Current;
            using (profiler.Step($"Calling GetClient"))
            {
                var serviceClient = new SearchServiceClient(_config.SearchServiceName,
                    new SearchCredentials(_config.SearchServiceAdminApiKey));
                return serviceClient;
            }
        }
    }
}
