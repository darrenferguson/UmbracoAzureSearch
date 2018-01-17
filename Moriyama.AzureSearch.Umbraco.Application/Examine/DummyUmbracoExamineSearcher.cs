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
        public override ISearchResults Search(ISearchCriteria searchCriteria)
        {
            var client = AzureSearchContext.Instance.SearchClient;

            //client.SetQueryType(QueryType.Full);
            //client.Filter("Published", true);
            //client.Filter("Trashed", false);

            var indexSet = IndexSets.Instance.Sets[IndexSetName];

            if (indexSet != null)
            {
                //client.Filter(GetExcludedDocTypesFilter(indexSet));
            }

            string query = GetLuceneQuery(searchCriteria);

            IAzureSearchQuery azureQuery =
                new AzureSearchQuery(query)
                    .QueryType(QueryType.Full);

            var azureResults = client.Results(azureQuery);

            ISearchResults azureExamineResults = new AzureExamineSearchResults(azureResults);
            return azureExamineResults;
        }

        private static string GetLuceneQuery(ISearchCriteria searchCriteria)
        {
            // this line can be used when examine dependency is updated 
            //if (searchCriteria is LuceneSearchCriteria criteria) return criteria.Query?.ToString();

            var query = Regex.Match(searchCriteria.ToString(), "LuceneQuery: (.*\\)) }");
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

