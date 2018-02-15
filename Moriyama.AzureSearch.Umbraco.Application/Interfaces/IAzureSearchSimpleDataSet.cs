using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchSimpleDataSet
    {
        int Id { get; set; }
        Dictionary<string, ISearchValue> RowData { get; set; }
    }
}