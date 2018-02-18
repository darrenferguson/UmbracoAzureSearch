using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchUmbracoIndexClient : IBaseAzureSearch
    {
        Field[] GetStandardUmbracoFields();
        
        AzureSearchReindexStatus ReIndexContent(string sessionId, int page);
        AzureSearchReindexStatus ReIndexMedia(string sessionId, int page);
        AzureSearchReindexStatus ReIndexMember(string sessionId, int page);

        AzureSearchReindexStatus ReIndex(string filename, string sessionId, int page);

        void ReIndexContent(IContent content);
        void ReIndexContent(IMedia content);
        void ReIndexMember(IMember content);

    }
}
