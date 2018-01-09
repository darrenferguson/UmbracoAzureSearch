﻿using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using System;
using System.Linq;
using log4net;
using System.Reflection;
using System.Web;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchClient : BaseAzureSearch, IAzureSearchClient
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AzureSearchClient(string path) : base(path)
        {}

        public ISearchResult Results(IAzureSearchQuery query)
        {
            var client = GetClient();
            var config = GetConfiguration();
            var sp = query.GetSearchParameters();
            ISearchIndexClient indexClient = client.Indexes.GetClient(config.IndexName);
            var startTime = DateTime.UtcNow;
            var response = indexClient.Documents.Search(query.Term, query.GetSearchParameters());

            var processStartTime = DateTime.UtcNow;
            var results = new Models.SearchResult();

            foreach (var result in response.Results)
            {
                results.Content.Add(FromDocument(result.Document, result.Score, query.PopulateContentProperties));
            }

            if (response.Facets != null)
            {
                foreach (var facet in response.Facets)
                {
                    var searchFacet = new SearchFacet()
                    {
                        Name = facet.Key,
                        Items = facet.Value.Select(x => new KeyValuePair<string, long>(x.Value.ToString(), x.Count.HasValue ? x.Count.Value : 0))
                    };

                    results.Facets.Add(searchFacet);
                }
            }

            if (response.Count != null)
            {
                results.Count = (int)response.Count;
            }

            if (config.LogSearchPerformance)
            {
                string lb = Environment.NewLine;
                Log.Info($"AzureSearch Log (cached client){lb} - Response Duration: {(int)(processStartTime - startTime).TotalMilliseconds}ms{lb} - Process Duration: {(int)(DateTime.UtcNow - processStartTime).TotalMilliseconds}ms{lb} - Results Count: {results.Count}{lb} - Origin: {HttpContext.Current?.Request?.Url}{lb} - Index name: {config.IndexName}{lb} - Base uri: {indexClient.BaseUri}{lb} - Search term: {query.Term}{lb} - Uri query string: {HttpUtility.UrlDecode(sp.ToString())}{lb}");
            }
            return results;
        }

        public IList<SuggestResult> Suggest(string value, int count, bool fuzzy = true)
        {
            var client = GetClient();
            var config = GetConfiguration();
            var indexClient = client.Indexes.GetClient(config.IndexName);
            var sp = new SuggestParameters()
            {
                UseFuzzyMatching = fuzzy,
                Top = count,
                Filter = "IsContent eq true"
            };

            return indexClient.Documents.Suggest(value, "sg", sp).Results;
        }

        private ISearchContent FromDocument(Document d, double score, bool populateContentProperties)
        {
            var searchContent = new SearchContent
            {
                Properties = new Dictionary<string, object>()
            };

            searchContent.Score = score;

            var t = searchContent.GetType();
            searchContent.Id = Convert.ToInt32(d["Id"]);

            foreach (var key in d.Keys)
            {
                var property = t.GetProperty(key);
                if (property == null && populateContentProperties)
                {
                    searchContent.Properties.Add(key, d[key]);
                }
                else if (property != null && (property.CanWrite && property.Name != "Id"))
                {

                    object val = d[key];

                    if (val == null)
                        continue;

                    if (val is long)
                        val = Convert.ToInt32(val);

                    if (val is DateTimeOffset)
                        val = ((DateTimeOffset)val).DateTime;

                    if (property.PropertyType == val.GetType())
                        property.SetValue(searchContent, val);
                }
            }

            return searchContent;
        }
    }
}
