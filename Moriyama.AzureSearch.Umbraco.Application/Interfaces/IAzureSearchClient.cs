using System.Collections;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchClient
    {

        IEnumerable<ISearchContent> Search(string query);
    }
}
