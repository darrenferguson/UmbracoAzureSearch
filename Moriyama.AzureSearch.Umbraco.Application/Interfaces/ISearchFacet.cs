using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ISearchFacet
    {
        long? Count { get; set; }
        string Name { get; set; }
    }
}
