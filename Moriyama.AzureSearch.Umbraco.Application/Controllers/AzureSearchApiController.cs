using System;
using System.Web.Http;
using System.Web.ModelBinding;
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

        [HttpPost]
        public AzureSearchReindexStatus ReIndex(ReIndexModel reIndexModel)
        {
            var status = GetStatus();

            _azureSearchServiceIndexClient.ReIndexContent(status.SessionId);

            if (reIndexModel.content)
            {
                GetReIndexContent(status.SessionId, 0);
            }

            if (reIndexModel.media)
            {
                GetReIndexMedia(status.SessionId, 0);
            }

            if (reIndexModel.members)
            {
                GetReIndexMember(status.SessionId, 0);
            }

            status.Message = "Preparing...";

            return status;
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

        private AzureSearchReindexStatus GetStatus()
        {
            var sessionKey = Security.CurrentUser.Key.ToString();

            return (AzureSearchReindexStatus)UmbracoContext.Application.ApplicationCache.RuntimeCache.GetCacheItem($"AzureReindex_{sessionKey}", () => new AzureSearchReindexStatus
            {
                SessionId = sessionKey
            });
        }
    }
}
