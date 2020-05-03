using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class SearchFacet : ISearchFacet
    {
        public string Name { get; set; }

        public IEnumerable<KeyValuePair<string, long>> Items { get; set; }
    }
}
