using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchIndexClient
    {
        AzureSearchConfig GetConfiguration();

        Field[] GetStandardUmbracoFields();
        Index[] GetSearchIndexes();

        string DropCreateIndex();
        
        AzureSearchReindexStatus ReIndexContent(string sessionId);
        AzureSearchReindexStatus ReIndexContent(string sessionId, int page);
        AzureSearchReindexStatus ReIndexMedia(string sessionId, int page);


        void ReIndexContent(IContent content);
        void ReIndexContent(IMedia content);
    }
}
