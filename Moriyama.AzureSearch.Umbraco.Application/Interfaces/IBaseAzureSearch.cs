using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IBaseAzureSearch
    {
        Index[] GetSearchIndexes();
        string DropCreateIndex();
        AzureSearchReindexStatus ReIndexSetup(string sessionId);
        SearchServiceClient GetClient();
        AzureSearchConfig GetConfiguration();
        void SaveConfiguration(AzureSearchConfig config);
    }
}