using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ISearchResult
    {
        int Count { get; set; }

        IList<ISearchContent> Content { get; set; }

        IList<ISearchFacet> Facets { get; set; }


    }
}
