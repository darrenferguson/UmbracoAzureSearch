using log4net;
using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public abstract class BaseAzureSearch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected AzureSearchConfig _configuration;
        protected readonly string _configurationFile;

        private readonly ISearchServiceClient _searchServiceClient;
        
        public BaseAzureSearch(string configurationFile)
        {
            this._configurationFile = configurationFile;

            try
            {
                this._configuration = JsonConvert.DeserializeObject<AzureSearchConfig>(File.ReadAllText(_configurationFile));

            } catch(FileNotFoundException fileNotFoundException)
            {
                Log.Fatal($"Config file not found {this._configurationFile}", fileNotFoundException);
                throw fileNotFoundException;

            } catch(FormatException formatException)
            {
                Log.Fatal($"Config file malformed {this._configurationFile}", formatException);
                throw formatException;
            }

            this._searchServiceClient = new SearchServiceClient(_configuration.SearchServiceName, new SearchCredentials(_configuration.SearchServiceAdminApiKey));
        }

        public void SaveConfiguration(AzureSearchConfig configuration)
        {
            this._configuration = configuration;
            File.WriteAllText(_configurationFile, JsonConvert.SerializeObject(configuration, Formatting.Indented));
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
