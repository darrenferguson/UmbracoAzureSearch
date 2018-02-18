using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public abstract class BaseAzureSearch
    {
        protected AzureSearchConfig _config;
        protected readonly string _path;
        protected readonly string _configPath;

        // Number of docs to be processed at a time.
        const int BatchSize = 999;

        public BaseAzureSearch(string path, string configPath)
        {
            _path = path;
            _configPath = configPath;
            InitConfig<AzureSearchConfig>(path, configPath);
        }

        public Index[] GetSearchIndexes()
        {
            var serviceClient = GetClient();
            var indexes = serviceClient.Indexes.List().Indexes;
            return indexes.ToArray();
        }

        public virtual string DropCreateIndex()
        {
            throw new NotImplementedException();
        }

        public virtual string Delete(string id)
        {
            throw new NotImplementedException();
        }

        public virtual AzureSearchReindexStatus ReIndexSetup(string sessionId)
        {
            throw new NotImplementedException();
        }

        protected virtual Config InitConfig<Config>(string path, string configPath)
            where Config : AzureSearchConfig
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(path, configPath)));
            _config = config;
            return config;
        }

        public void SaveConfiguration(AzureSearchConfig config)
        {
            _config = config;
            File.WriteAllText(Path.Combine(_path, _configPath), JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public AzureSearchConfig GetConfiguration()
        {
            return _config;
        }

        public SearchServiceClient GetClient()
        {
            var serviceClient = new SearchServiceClient(_config.SearchServiceName, new SearchCredentials(_config.SearchServiceAdminApiKey));
            return serviceClient;
        }

        public SearchIndexClient GetSearcher()
        {
            var config = GetConfiguration();
            var client = GetClient();
            return client.Indexes.GetClient(config.IndexName);
        }
    }
}
