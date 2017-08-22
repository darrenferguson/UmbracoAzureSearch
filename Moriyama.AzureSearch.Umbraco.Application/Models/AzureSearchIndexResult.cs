namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class AzureSearchIndexResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int DocumentsProcessed { get; set; }
    }
}
