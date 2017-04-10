using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class SearchFacet : ISearchFacet
    {
        public long? Count { get; set; }

        public string Name { get; set; }
    }
}
