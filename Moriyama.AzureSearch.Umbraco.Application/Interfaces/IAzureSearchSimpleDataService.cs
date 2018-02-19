using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchSimpleDataService
    {
        List<int> GetAllIds();
        IEnumerable<IAzureSearchSimpleDataSet> GettBatchData(int[] ids);
    }
}
