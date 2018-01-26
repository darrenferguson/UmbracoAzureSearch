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

        // Number of docs to be processed at a time.
        const int BatchSize = 999;

        public BaseAzureSearch(string path)
        {
            _path = path;

            var deserializationSettings = GetClient().DeserializationSettings;

            var configData = File.ReadAllText(Path.Combine(path, @"config\AzureSearch.config"));

            _config = JsonConvert.DeserializeObject<AzureSearchConfig>(configData, deserializationSettings);
        }

        public void SaveConfiguration(AzureSearchConfig config)
        {
            _config = config;
            var serializerSettings = GetSerializationSettings(config);
            serializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

            File.WriteAllText(Path.Combine(_path, @"config\AzureSearch.config"), JsonConvert.SerializeObject(config, Formatting.Indented, serializerSettings));
        }

        public AzureSearchConfig GetConfiguration()
        {
            return _config;
        }

        public static SearchServiceClient GetClient(AzureSearchConfig config = null)
        {
            var serviceName = config != null ? config.SearchServiceName : "name";
            var apiKey = config != null ? config.SearchServiceAdminApiKey : "apikey";

            var serviceClient = new SearchServiceClient(serviceName, new SearchCredentials(apiKey));
            return serviceClient;
        }

        public static JsonSerializerSettings GetSerializationSettings(AzureSearchConfig config = null)
        {
            return GetClient(config).SerializationSettings;
        }
    }
}
