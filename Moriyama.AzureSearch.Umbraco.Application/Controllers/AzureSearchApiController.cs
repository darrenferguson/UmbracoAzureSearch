﻿using System;
using System.Web;
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
        private readonly IAzureSearchIndexClient _azureSearchServiceClient;

        public AzureSearchApiController()
        {
            _azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;
        }

        public AzureSearchConfig GetConfiguration()
        {
            return _azureSearchServiceClient.GetConfiguration();
        }

        [HttpPost]
        public AzureSearchConfig SetConfiguration(AzureSearchConfig config)
        {
            _azureSearchServiceClient.SaveConfiguration(config);

            return _azureSearchServiceClient.GetConfiguration();
        }

        [HttpGet]
        public bool ServiceName(string value)
        {
            var config = _azureSearchServiceClient.GetConfiguration();
            config.SearchServiceName = value;
            _azureSearchServiceClient.SaveConfiguration(config);
            return true;
        }

        [HttpGet]
        public bool ServiceApiKey(string value)
        {
            var config = _azureSearchServiceClient.GetConfiguration();
            config.SearchServiceAdminApiKey = value;
            _azureSearchServiceClient.SaveConfiguration(config);
            return true;
        }

        public string GetTestConfig()
        {

            var config = _azureSearchServiceClient.GetConfiguration();

            int indexCount = 0;

            try
            {
                var serviceClient = new SearchServiceClient(config.SearchServiceName, new SearchCredentials(config.SearchServiceAdminApiKey));
                var indexes = serviceClient.Indexes.List();
                indexCount = indexes.Indexes.Count;

            } 
            catch (Exception ex)
            {
                return "Cannot connect: " + ex.Message;
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

        private AzureSearchReindexStatus GetStatus()
        {
            var sessionKey = Security.CurrentUser.Key.ToString();

            return (AzureSearchReindexStatus)UmbracoContext.Application.ApplicationCache.RuntimeCache.GetCacheItem($"AzureReindex_{sessionKey}", () => new AzureSearchReindexStatus
            {
                SessionId = sessionKey
            });
        }

        [HttpPost]
        public AzureSearchReindexStatus ReIndex(ReIndexModel reIndexModel)
        {
            var status = GetStatus();

            

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

        public AzureSearchReindexStatus GetReIndexContent(string sessionId, int page)
        {
            var status = GetStatus();
            var result = _azureSearchServiceClient.ReIndexContent(sessionId, page);
            status.DocumentsQueued = result.DocumentsQueued;
            status.Message = result.Message;
            status.Error = result.Error;

            return status;
        }

        public AzureSearchReindexStatus GetReIndexMedia(string sessionId, int page)
        {
            var status = GetStatus();
            var result = _azureSearchServiceClient.ReIndexMedia(sessionId, page);
            status.MediaQueued = result.MediaQueued;
            status.Message = result.Message;
            status.Error = result.Error;

            return status;
        }

        public AzureSearchReindexStatus GetReIndexMember(string sessionId, int page)
        {
            var status = GetStatus();
            var result = _azureSearchServiceClient.ReIndexMember(sessionId, page);
            status.MembersQueued = result.MembersQueued;
            status.Message = result.Message;
            status.Error = result.Error;

            return status;
        }
    }
}
