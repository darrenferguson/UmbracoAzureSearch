using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Web.WebApi;
using System.Web.Http;

namespace Moriyama.AzureSearch.Umbraco.Application.Controllers
{
    public class AzureSearchApiController : UmbracoAuthorizedApiController
    {
        private readonly Dictionary<string, IBaseAzureSearch> _azureSearchServiceClients;
        private readonly AzureSearchUmbracoIndexClient _umbracoSearchServiceClient;

        public AzureSearchApiController()
        {
            _azureSearchServiceClients = AzureSearchContext.Instance.SearchIndexClientCollection;
            _umbracoSearchServiceClient = AzureSearchContext.Instance.UmbracoIndexClient;
        }

        public string[] GetIndexers()
        {
            return _azureSearchServiceClients.Select(x => x.Key).ToArray();
        }

        public AzureSearchConfig GetConfiguration(string name = AzureSearchConstants.UmbracoIndexName)
        {
            return _azureSearchServiceClients[name].GetConfiguration();
        }

        [HttpGet]
        public bool ServiceName(string value, string name = AzureSearchConstants.UmbracoIndexName)
        {
            var config = _azureSearchServiceClients[name].GetConfiguration();
            config.SearchServiceName = value;
            _azureSearchServiceClients[name].SaveConfiguration(config);
            return true;
        }

        [HttpGet]
        public bool ServiceApiKey(string value, string name = AzureSearchConstants.UmbracoIndexName)
        {
            var config = _azureSearchServiceClients[name].GetConfiguration();
            config.SearchServiceAdminApiKey = value;
            _azureSearchServiceClients[name].SaveConfiguration(config);
            return true;
        }

        public string GetTestConfig(string name = AzureSearchConstants.UmbracoIndexName)
        {

            var config = _azureSearchServiceClients[name].GetConfiguration();

            int indexCount = 0;

            try
            {
                var serviceClient = new SearchServiceClient(config.SearchServiceName, new SearchCredentials(config.SearchServiceAdminApiKey));
                var indexes = serviceClient.Indexes.List();
                indexCount = indexes.Indexes.Count;

            } catch
            {
                return "Cannot connect";
            }

            return "Connected and got " + indexCount + " indexes";
        }

        public SearchField[] GetStandardFields(string name = AzureSearchConstants.UmbracoIndexName)
        {
            if(name == AzureSearchConstants.UmbracoIndexName)
                return Mapper.Map<SearchField[]>(_umbracoSearchServiceClient.GetStandardUmbracoFields());
            else
            {
                var simpleData = _azureSearchServiceClients[name] as IAzureSearchSimpleDataSetIndexClient;
                if (simpleData != null)
                    return Mapper.Map<SearchField[]>(simpleData.GetStandardFields());
                else
                    return new SearchField[] { };
            }
        }

        public SearchIndex[] GetSearchIndexes(string name = AzureSearchConstants.UmbracoIndexName)
        {
            return Mapper.Map<SearchIndex[]>(_azureSearchServiceClients[name].GetSearchIndexes());
        }

        public string GetDropCreateIndex(string name = AzureSearchConstants.UmbracoIndexName)
        {
            return _azureSearchServiceClients[name].DropCreateIndex();
        }

        public AzureSearchReindexStatus GetReIndexSetup(string name = AzureSearchConstants.UmbracoIndexName)
        {
            var sessionId = Guid.NewGuid().ToString();
            var result = _azureSearchServiceClients[name].ReIndexSetup(sessionId);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexContent(string sessionId, int page)
        {
            var result = _umbracoSearchServiceClient.ReIndexContent(sessionId, page);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexMedia(string sessionId, int page)
        {
            var result = _umbracoSearchServiceClient.ReIndexMedia(sessionId, page);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexMember(string sessionId, int page)
        {
            var result = _umbracoSearchServiceClient.ReIndexMember(sessionId, page);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexExternal(string sessionId, int page, string name)
        {
            var indexer = _azureSearchServiceClients[name] as AzureSearchSimpleDatasetIndexClient;
            if(indexer != null)
            {
                var result = indexer.ReIndex(sessionId, page);
                return result;
            }
            return new AzureSearchReindexStatus() { Error = true };
        }

    }
}
