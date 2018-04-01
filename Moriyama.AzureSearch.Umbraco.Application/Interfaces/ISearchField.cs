namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{


    public interface ISearchField : IAzureSearchField
    {
        bool IsGridJson { get; set; }
        string ParserType { get; set; }
    }
}
