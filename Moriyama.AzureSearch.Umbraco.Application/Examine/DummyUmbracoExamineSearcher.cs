using System.IO;
using Examine;
using Examine.SearchCriteria;
using System.Linq;
using System.Text.RegularExpressions;
using Examine.LuceneEngine.Config;
using UmbracoExamine;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Examine
{
    public partial class DummyUmbracoExamineSearcher : UmbracoExamineSearcher
    {

        public DummyUmbracoExamineSearcher()
        {
            
        }

        // Need this constructor for tests...
        public DummyUmbracoExamineSearcher(Lucene.Net.Store.Directory luceneDirectory,
            Lucene.Net.Analysis.Analyzer analyzer) : base(luceneDirectory, analyzer)
        {

        }

        public override ISearchResults Search(string searchTerm, bool useWildCards)
        {
            AzureSearchContext azureSearchContext = AzureSearchContext.Instance;
            IAzureSearchQuery query = new AzureSearchQuery(searchTerm);

            var results = azureSearchContext.SearchClient.Results(query);
            ISearchResults azureExamineResults = new AzureExamineSearchResults(results);

            return azureExamineResults;
        }

        public override ISearchResults Search(ISearchCriteria searchCriteria)
        {
            var client = AzureSearchContext.Instance.SearchClient;

            string query = GetLuceneQuery(searchCriteria);

            IAzureSearchQuery azureQuery =
                new AzureSearchQuery(query)
                    .QueryType(QueryType.Full);

            // TODO - this would be set at indexer level?
            //client.Filter("Published", true);
            //client.Filter("Trashed", false);

            if (IndexSets.Instance != null && IndexSets.Instance.Sets != null &&
                IndexSets.Instance.Sets[IndexSetName] != null)
            {

                // TODO: Work out with Tom.
                //azureQuery.Filter(GetExcludedDocTypesFilter(IndexSets.Instance.Sets[IndexSetName]));
            }

            var azureResults = client.Results(azureQuery);

            ISearchResults azureExamineResults = new AzureExamineSearchResults(azureResults);
            return azureExamineResults;
        }

        private static string GetLuceneQuery(ISearchCriteria searchCriteria)
        {
            // this line can be used when examine dependency is updated 
            //if (searchCriteria is LuceneSearchCriteria criteria) return criteria.Query?.ToString();
            var query = Regex.Match(searchCriteria.ToString(), ".*?LuceneQuery: (.*)\\}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return query.Success && query.Groups.Count > 0 ? query.Groups[1].Value : string.Empty;;
        }

        private static string GetExcludedDocTypesFilter(IndexSet indexSet)
        {
            var excludeDocs = indexSet?.ExcludeNodeTypes?.ToList();
            if (excludeDocs?.Any() == false) return string.Empty;

            var docNames = excludeDocs?.Select(i => i?.Name) ?? Enumerable.Empty<string>();
            return $"not search.in(ContentTypeAlias, '{string.Join(", ", docNames)}')";
        }
    }
}

