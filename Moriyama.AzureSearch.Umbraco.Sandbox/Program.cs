using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var config = JsonConvert.DeserializeObject<AzureSearchConfig>(File.ReadAllText("config/AzureSearch.config"));

            SearchServiceClient serviceClient = new SearchServiceClient(config.SearchServiceName, new SearchCredentials(config.SearchServiceAdminApiKey));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("umbraco");

            // SearchDocuments(indexClient, "umbraco");
            SearchDocuments(indexClient, "*", filter: "ContentTypeAlias eq 'TextPage'");

            // TODO:
            // All Documents by type
            // Sort
            // IPublishedContent<>
            // 
            // IsProtected - Index
            // Search by Path
            // Paging
        }

        private static void SearchDocuments(ISearchIndexClient indexClient, string searchText, string filter = null)
        {
            // Execute search based on search text and optional filter
            var sp = new SearchParameters();

            if (!String.IsNullOrEmpty(filter))
            {
                sp.Filter = filter;
            }

            var response = indexClient.Documents.Search(searchText, sp);

            foreach (var result in response.Results)
            {
                Console.WriteLine(result.Document["Name"] + " " + result.Document["Id"]);
            }
        }
    }
}
