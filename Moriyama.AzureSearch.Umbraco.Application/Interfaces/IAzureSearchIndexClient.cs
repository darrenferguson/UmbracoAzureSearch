using System;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchIndexClient
    {
        AzureSearchConfig GetConfiguration();
        void SaveConfiguration(AzureSearchConfig config);

        Field[] GetStandardUmbracoFields();
        Index[] GetSearchIndexes();

        string DropCreateIndex();
        
        [Obsolete]
        AzureSearchReindexStatus ReIndexContent(string sessionId);
        AzureSearchReindexStatus ReIndexContent(string sessionId, int page);
        AzureSearchReindexStatus ReIndexMedia(string sessionId, int page);
        AzureSearchReindexStatus ReIndexMember(string sessionId, int page);
        
        void ReIndexContent(IContent content, bool raiseContentIndexingEvent = true);
        void ReIndexContent(IMedia content, bool raiseContentIndexingEvent = true);
        void ReIndexMember(IMember content, bool raiseContentIndexingEvent = true);

        void Delete(int id);

    }
}
