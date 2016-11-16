using System;
using System.Web;
using AutoMapper;
using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Web.WebApi;

namespace Moriyama.AzureSearch.Umbraco.Application.Controllers
{
    public class AzureSearchApiController : UmbracoAuthorizedApiController
    {
        private readonly IAzureSearchIndexClient _azureSearchServiceClient;

        public AzureSearchApiController()
        {
            _azureSearchServiceClient = new AzureSearchServiceClient(HttpContext.Current.Server.MapPath("/"));
        }

        public AzureSearchConfig GetConfiguration()
        {
            return _azureSearchServiceClient.GetConfiguration();
        }

        public string GetTestConfig()
        {

            var config =  _azureSearchServiceClient.GetConfiguration();

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
            return Mapper.Map<SearchField[]>(_azureSearchServiceClient.GetStandardUmbracoFields());
        }

        public SearchIndex[] GetSearchIndexes()
        {
            return Mapper.Map<SearchIndex[]>(_azureSearchServiceClient.GetSearchIndexes());
        }

        public string GetDropCreateIndex()
        {
            return _azureSearchServiceClient.DropCreateIndex();
        }

        public AzureSearchReindexStatus GetReIndexContent()
        {
            var sessionId = Guid.NewGuid().ToString();
            var result = _azureSearchServiceClient.ReIndexContent(sessionId);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexContent(string sessionId, int page)
        {
            var result = _azureSearchServiceClient.ReIndexContent(sessionId, page);
            return result;
        }

        public AzureSearchReindexStatus GetReIndexMedia(string sessionId, int page)
        {
            var result = _azureSearchServiceClient.ReIndexMedia(sessionId, page);
            return result;
        }

    }
}
