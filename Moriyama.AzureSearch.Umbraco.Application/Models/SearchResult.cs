using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class SearchResult : ISearchResult
    {
        public SearchResult()
        {
            Content = new List<ISearchContent>();
            Facets = new List<ISearchFacet>();
        }

        public int Count { get; set; }

        public IList<ISearchContent> Content { get; set; }

        public IList<ISearchFacet> Facets { get; set; }
    }
}
