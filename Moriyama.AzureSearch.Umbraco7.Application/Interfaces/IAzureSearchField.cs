namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchField
    {
        string Name { get; set; }
        string Type { get; set; }

        // Azure config
        bool IsKey { get; set; }
        bool IsSortable { get; set; }
        bool IsSearchable { get; set; }
        bool IsFacetable { get; set; }
        bool IsFilterable { get; set; }
    }
}
