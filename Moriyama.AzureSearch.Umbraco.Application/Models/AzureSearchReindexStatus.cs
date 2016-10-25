namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class AzureSearchReindexStatus
    {

        public string SessionId { get; set; }

        public int DocumentCount { get; set; }
        public int DocumentsProcessed { get; set; }

        public bool Error { get; set; }
        public bool Finished { get; set; }

        public string Message { get; set; }
    }
}
