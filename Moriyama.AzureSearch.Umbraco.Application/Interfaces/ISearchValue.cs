using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ISearchValue
    {
        string String { get; set; }
        int Int { get; set; }
        int Bool { get; set; }
        List<string> Collection { get; set; }
        DateTime DateTime { get; set; }
    }
}
