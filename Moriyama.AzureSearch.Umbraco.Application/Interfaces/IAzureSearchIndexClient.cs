﻿using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models.Result;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchIndexClient
    {
        AzureSearchConfig GetConfiguration();
        
        Field[] GetStandardUmbracoFields();
        Index[] GetSearchIndexes();

        CreateIndexResult DropCreateIndex();
        DropIndexResult DropIndex(string indexName); 

        AzureSearchReindexStatus ReIndexContent(string sessionId);
        AzureSearchReindexStatus ReIndexContent(string sessionId, int page);
        AzureSearchReindexStatus ReIndexMedia(string sessionId, int page);
        AzureSearchReindexStatus ReIndexMember(string sessionId, int page);

        void ReIndexContent(IContent content);
        void ReIndexMedia(IMedia content);
        void ReIndexMember(IMember content);

        AzureSearchIndexResult Delete(int id);
    }
}
