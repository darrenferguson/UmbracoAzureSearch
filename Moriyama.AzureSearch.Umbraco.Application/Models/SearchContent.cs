using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class SearchContent : ISearchContent
    {
        public int Id { get; set; }

        public string Name { get; set; }

    }
}
