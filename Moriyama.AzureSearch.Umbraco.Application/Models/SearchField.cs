namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class SearchField
    {
        public string Name { get; set; }
        public string Type { get; set; }

        // Azure config
        public bool IsKey { get; set; }
        public bool IsSortable { get; set; }
        public bool IsFacetable { get; set; }
        public bool IsFilterable { get; set; }

        // Custom/Umbraco config
        public bool IsJson { get; set; }
        public bool IsCommaDelimited { get; set; }
    }
}
