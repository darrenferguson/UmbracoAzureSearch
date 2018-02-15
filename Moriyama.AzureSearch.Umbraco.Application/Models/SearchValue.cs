using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class SearchValue : ISearchValue
    {
        public string String { get; set; }
        public int Int { get; set; }
        public int Bool { get; set; }
        public List<string> Collection { get; set; }
        public DateTime DateTime { get; set; }
    }
}
