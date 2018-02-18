using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IBaseAzureSearch
    {
        void SaveConfiguration(AzureSearchConfig config);

        Index[] GetSearchIndexes();
        string DropCreateIndex();
        AzureSearchReindexStatus ReIndexSetup(string sessionId);
        void Delete(string id);

        SearchServiceClient GetClient();
        AzureSearchConfig GetConfiguration();
        SearchIndexClient GetSearcher();
    }
}