using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ISearchFacet
    {
        string Name { get; set; }
        IEnumerable<KeyValuePair<string, long>> Items { get; set; }
    }
}
