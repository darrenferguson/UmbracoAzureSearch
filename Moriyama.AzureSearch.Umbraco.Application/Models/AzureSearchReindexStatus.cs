namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class AzureSearchReindexStatus
    {

        public string SessionId { get; set; }

        public int DocumentsQueued { get; set; }
        public int MediaQueued { get; set; }
        public int MembersQueued { get; set; }

        public bool Error { get; set; }
        public bool Finished { get; set; }

        public string Message { get; set; }
    }
}
