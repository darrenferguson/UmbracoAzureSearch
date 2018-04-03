using System;
using AutoMapper;
using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models.Result;
using Umbraco.Web.WebApi;

namespace Moriyama.AzureSearch.Umbraco.Application.Controllers
{
    public class AzureSearchApiController : UmbracoAuthorizedApiController
    {
        private readonly IAzureSearchIndexClient _azureSearchServiceIndexClient;

        public AzureSearchApiController()
        {
            this._azureSearchServiceIndexClient = AzureSearchContext.Instance.SearchIndexClient;
        }

        public AzureSearchConfig GetConfiguration()
        {
            return this._azureSearchServiceIndexClient.GetConfiguration();
        }
            
        public string GetTestConfig()
        {
            AzureSearchConfig config =  this._azureSearchServiceIndexClient.GetConfiguration();

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

        public SearchField[] GetStandardUmbracoFields()
        {
            return Mapper.Map<SearchField[]>(this._azureSearchServiceIndexClient.GetStandardUmbracoFields());
        }

        public SearchIndex[] GetSearchIndexes()
        {
            return Mapper.Map<SearchIndex[]>(this._azureSearchServiceIndexClient.GetSearchIndexes());
        }

        public CreateIndexResult GetDropCreateIndex()
        {
            return this._azureSearchServiceIndexClient.DropCreateIndex();
        }

        public AzureSearchReindexStatus GetReIndexContent()
        {
            var sessionId = Guid.NewGuid().ToString();
            var result = this._azureSearchServiceIndexClient.ReIndexContent(sessionId);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexContent(string sessionId, int page)
        {
            var result = this._azureSearchServiceIndexClient.ReIndexContent(sessionId, page);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexMedia(string sessionId, int page)
        {
            var result = this._azureSearchServiceIndexClient.ReIndexMedia(sessionId, page);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexMember(string sessionId, int page)
        {
            var result = this._azureSearchServiceIndexClient.ReIndexMember(sessionId, page);
            return result;
        }
    }
}
