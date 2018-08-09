using Microsoft.Azure.Search.Models;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchClient
    {
        IAzureSearchQuery CreateQuery(string term);

        ISearchResult Results(IAzureSearchQuery azureSearchQuery);

        IList<SuggestResult> Suggest(string value, string suggesterName, int count, bool fuzzy = true);
    }
}
