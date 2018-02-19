using System.Collections.Generic;
using System.Text;
using Umbraco.Web;
using Moriyama.AzureSearch.Umbraco.Application;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace Moriyama.AzureSearch.Umbraco.ExampleSimpleData
{
    public class SimpleDataSetExampleSearch
    { 

        public IList<SearchResult> Apply(List<string> searchTerms)
        {
            // Perform the search
            var indexer = AzureSearchContext.Instance.SearchIndexClientCollection["simpledata"];
            var searcher = indexer.GetSearcher();

            var query = new StringBuilder();

            var searchFields = new List<string>() { "Name" };

            // Rank content based on positon of search terms in fields
            for (var i = 0; i < searchFields.Count; i++)
            {
                foreach (var term in searchTerms)
                {
                    query.AppendFormat("{0}:*{1}*^{2} ", searchFields[i], term, searchFields.Count - i);
                }
            }

            var parameters =
                new SearchParameters()
                {
                    Top = 200,
                    SearchFields = searchFields,
                };

            return searcher.Documents.Search(query.ToString(), parameters).Results;
               
        }
    }
}
