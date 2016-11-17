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

        private IList<string> _filters;
        private string _conjunctive = " and ";
        private string _searchTerm = "*";
        private IList<string> _orderBy;

        private bool _content;
        private bool _media;

        private int _pageSize;
        private bool _populateContentProperties = true;

        public AzureSearchClient(string path) : base(path)
        {
            _pageSize = 999;

            _filters = new List<string>();
            _orderBy = new List<string>();
        }
        
        public IAzureSearchClient DocumentType(string typeAlias)
        {
            _filters.Add(string.Format("ContentTypeAlias eq '{0}'", typeAlias));
            return this;
        }

        private SearchParameters GetSearchParameters()
        {
            var sp = new SearchParameters();

            if(_content)
            {
                _filters.Add("IsContent eq true");
            }

            if(_media)
            {
                _filters.Add("IsMedia eq true");
            }

            if (_filters.Count > 0)
            {
                sp.Filter = string.Join(_conjunctive, _filters);
            }

            sp.OrderBy = _orderBy;
            return sp;
        }

        public IEnumerable<ISearchContent> Results()
        {       
            var sp = GetSearchParameters();
            return Results(sp);        
        }

        private IEnumerable<ISearchContent> Results(SearchParameters sp)
        {
            var client = GetClient();
            var config = GetConfiguration();

            ISearchIndexClient indexClient = client.Indexes.GetClient(config.IndexName);
            var response = indexClient.Documents.Search(_searchTerm, sp);

            var results = new List<ISearchContent>();

            foreach (var result in response.Results)
            {              
                results.Add(FromDocument(result.Document));
            }

            return results;
        }

        private ISearchContent FromDocument(Document d)
        {
            var searchContent = new SearchContent();
            searchContent.Properties = new Dictionary<string, object>();


            var t = searchContent.GetType();
            searchContent.Id = Convert.ToInt32(d["Id"]);

            foreach (var key in d.Keys)
            {
                var property = t.GetProperty(key);
                if(property == null && _populateContentProperties)
                {
                    searchContent.Properties.Add(key, d[key]);
                } else if(property.CanWrite && property.Name != "Id")
                {

                    object val = d[key];

                    if (val == null)
                        continue;

                    if (val.GetType() == typeof(System.Int64))
                        val = Convert.ToInt32(val);

                    if (val.GetType() == typeof(System.DateTimeOffset))
                        val = ((DateTimeOffset)val).DateTime;

                    if (property.PropertyType == val.GetType())
                        property.SetValue(searchContent, val);
                }
            }
                        
            return searchContent;
        }

        public IEnumerable<ISearchContent> Results(int page)
        {

            var sp = GetSearchParameters();
            sp.Top = _pageSize;
            sp.Skip = (page -1) * _pageSize;

            return Results(sp);
        }
        
        public IAzureSearchClient Term(string query)
        {
            _searchTerm = query;
            return this;
        }

        public IAzureSearchClient OrderBy(string fieldName)
        {
            _orderBy.Add(fieldName);
            return this;
        }

        public IAzureSearchClient PageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public IAzureSearchClient Content()
        {
            _content = true;
            return this;
        }

        public IAzureSearchClient Media()
        {
            _media = true;
            return this;
        }

        public IAzureSearchClient PopulateContentProperties(bool populate)
        {
            _populateContentProperties = populate;
            return this;
        }
    }
}
