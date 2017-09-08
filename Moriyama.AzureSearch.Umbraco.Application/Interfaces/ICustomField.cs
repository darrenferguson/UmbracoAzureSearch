namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ICustomField : IAzureSearchField
    {
        string ParserType { get; set; }
    }
}
