namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public enum FieldType
    {
        Int, String, Collection, Bool, Date
    }

    public interface IAzureSearchField
    {
        string Name { get; set; }
        FieldType FieldType { get; set; }

        // Azure config
        bool IsKey { get; set; }
        bool IsSortable { get; set; }
        bool IsSearchable { get; set; }
        bool IsFacetable { get; set; }
        bool IsFilterable { get; set; }
    }
}
