using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class CustomField : ICustomField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsKey { get; set; }
        public bool IsSortable { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsFacetable { get; set; }
        public bool IsFilterable { get; set; }
        public string ParserType { get; set; }
    }
}
