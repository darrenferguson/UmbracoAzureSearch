using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchSimpleDataSetIndexClient : IBaseAzureSearch
    {
        AzureSearchReindexStatus ReIndex(string sessionId, int page);

        void ReIndexItem(IAzureSearchSimpleDataSet data);

        Field[] GetStandardFields();

    }
}