using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchSimpleDataSet : IAzureSearchSimpleDataSet
    {

        public int Id { get; set; }

        public Dictionary<string, ISearchValue> RowData { get; set; }

    }
}
