using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{

    public class SearchField : ISearchField
    {
        public string Name { get; set; }
        public FieldType FieldType { get; set; }
        public bool IsKey { get; set; }
        public bool IsSortable { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsFacetable { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsGridJson { get; set; }
        public string ParserType { get; set; }
        public string Analyzer { get; set; }
        public bool IsNestedLink { get; set; }

    }
}
