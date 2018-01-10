using System;
using System.Collections.Generic;
using Examine;
using Examine.SearchCriteria;
using System.IO;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Web.Models.ContentEditing;
using UmbracoExamine;
using Lucene.Net.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using StackExchange.Profiling;
using Umbraco.Core.Logging;

namespace Moriyama.AzureSearch.Umbraco.Application.Examine
{
    public partial class DummyUmbracoExamineSearcher : UmbracoExamineSearcher
    {
        public override ISearchResults Search(ISearchCriteria searchCriteria)
        {
            try
            {
                if (searchCriteria != null)
                {
                    var client = AzureSearchContext.Instance?.GetSearchClient();
                    if (client != null)
                    {
                        client.SetQueryType(QueryType.Full);
                        client.Filter("Published", true);
                        client.Filter("Trashed", false);

                        var indexSet = IndexSets.Instance?.Sets?[IndexSetName];
                        if (indexSet != null)
                        {
                            client.Filter(GetExcludedDocTypesFilter(indexSet));
                        }
                        
                        var query = GetLuceneQuery(searchCriteria);
                        var azQuery = client.Term(query);
                        var azureResults = azQuery?.Results();

                        ISearchResults azureExamineResults = new AzureExamineSearchResults(azureResults);
                        return azureExamineResults;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(GetType(), ex.Message, ex);
            }

            // Doing this will make Umbraco fallback to the database.
            throw new FileNotFoundException("");
        }

        private static string GetLuceneQuery(ISearchCriteria searchCriteria)
        {
            if (searchCriteria is LuceneSearchCriteria criteria) return criteria.Query?.ToString();

            var criteriaString = searchCriteria?.ToString().Replace("LuceneQuery: (", "LuceneQuery: \"(").Replace(") }", ")\" }");
            var query = JsonConvert.DeserializeObject<Query>(criteriaString);
            return query?.LuceneQuery;
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

