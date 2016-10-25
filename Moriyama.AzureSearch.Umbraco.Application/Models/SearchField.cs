namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class SearchField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsKey { get; set; }
        public bool IsSortable { get; set; }
        public bool IsFacetable { get; set; }
        public bool IsFilterable { get; set; }

    }
}
