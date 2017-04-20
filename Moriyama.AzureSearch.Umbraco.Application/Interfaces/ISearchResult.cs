using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ISearchResult
    {
        int Count { get; set; }

        IList<ISearchContent> Content { get; set; }

        IList<ISearchFacet> Facets { get; set; }
    }
}
