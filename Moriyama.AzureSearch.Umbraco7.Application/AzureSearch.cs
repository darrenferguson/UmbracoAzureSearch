using Microsoft.Azure.Search.Models;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearch
    {
        public static event AzureSearchEventHandler ContentIndexing;
        public static event AzureSearchEventHandler ContentIndexed;
        
        internal static bool FireContentIndexing(AzureSearchEventArgs e)
        {
            if (ContentIndexing != null)
                ContentIndexing(e);

            return e.Cancel;
        }
        
        internal static bool FireContentIndexed(AzureSearchEventArgs e)
        {
            if (ContentIndexed != null)
                ContentIndexed(e);

            return e.Cancel;
        }
    }

    public delegate void AzureSearchEventHandler(AzureSearchEventArgs e);

    public class AzureSearchEventArgs
    {
        public IContentBase Item { get; set; }
        public Document Entry { get; set; }
		public string EventSource { get; set; }
        public bool Cancel { get; set; }
    }
}