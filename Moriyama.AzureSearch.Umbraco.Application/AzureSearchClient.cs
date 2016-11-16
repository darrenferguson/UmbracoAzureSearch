using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using System;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchClient : BaseAzureSearch, IAzureSearchClient
    {
        public AzureSearchClient(string path) : base(path)
        {

        }

        public IEnumerable<ISearchContent> Search(string query)
        {
            var client = GetClient();
            var config = GetConfiguration();
            ISearchIndexClient indexClient = client.Indexes.GetClient(config.IndexName);

            var sp = new SearchParameters();

            //if (!String.IsNullOrEmpty(filter))
            //{
            //    sp.Filter = filter;
            //}

            var response = indexClient.Documents.Search(query, sp);

            var results = new List<ISearchContent>();

            foreach (var result in response.Results)
            {
                var searchContent = new SearchContent();

                searchContent.Id = Convert.ToInt32(result.Document["Id"]);
                searchContent.Name = (string) result.Document["Name"];
                          
                results.Add(searchContent);
            }

            return results;
        }
    }
}
