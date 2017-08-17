namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public sealed class BoolSearchFacet
    {
        public string Name { get; set; }

        public long TrueCount { get; set; }

        public long FalseCount { get; set; }
    }
}
