using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Examine;
using Examine.SearchCriteria;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Examine.Config;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Providers;
using Umbraco.Web.Models.ContentEditing;
using UmbracoExamine;
using Lucene.Net.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using StackExchange.Profiling;
using Umbraco.Core.Logging;

namespace Moriyama.AzureSearch.Umbraco.Application.Examine
{
    public class AzureSearchExamineProxySearcher : UmbracoExamineSearcher
    {
        private Lazy<BaseUmbracoIndexer> _indexerLazy;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            _indexerLazy = new Lazy<BaseUmbracoIndexer>(() =>
            {
                foreach (BaseUmbracoIndexer indexer in ExamineManager.Instance.IndexProviderCollection)
                {
                    if (indexer.IndexSetName == IndexSetName)
                    {
                        return indexer;
                    }
                }

                return null;
            });
        }

        protected override string[] GetSearchFields()
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath("~/");
            var serviceClient = new AzureSearchIndexClient(path);

            var systemFields = serviceClient.GetStandardUmbracoFields().Where(f => f.IsSearchable).Select(f => f.Name);
            var configFields = serviceClient.GetConfiguration().SearchFields.Where(f => f.IsSearchable).Select(f => f.Name);

            return systemFields.Concat(configFields).ToArray();
        }

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

                        var query = GetLuceneQuery(searchCriteria).Replace("__NodeId", "Id");

                        // fix trashed queries
                        if (query.Contains("-__Path:-1,-21,*"))
                        {
                            query = query.Replace("-__Path:-1,-21,*", "");
                            client.Filter("Trashed", false);
                        }
                        
                        switch (searchCriteria.SearchIndexType)
                        {
                            case "media":
                                client.Media();
                                query = query.Replace("+__IndexType:media", "");
                                break;

                            case "content":
                                client.Content();

                                // handle support unpublishedcontent
                                var onlyPublished = _indexerLazy.Value?.SupportUnpublishedContent == false;
                                if (onlyPublished)
                                {
                                    client.Filter("Published", true);
                                }
                                break;

                            case "member":
                                client.Filter("IsMember", true);
                                break;
                        }

                        var indexSet = IndexSets.Instance?.Sets?[IndexSetName];
                        if (indexSet != null)
                        {
                            var docTypes = GetExcludedDocTypesFilter(indexSet);
                            if (!string.IsNullOrEmpty(docTypes))
                            {
                                client.Filter(docTypes);
                            }
                        }
                        
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
            // this line can be used when examine dependency is updated 
            //if (searchCriteria is LuceneSearchCriteria criteria) return criteria.Query?.ToString();

            var query = Regex.Match(searchCriteria.ToString(), "LuceneQuery: (.*) }");
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

