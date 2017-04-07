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

        public IAzureSearchClient DocumentTypes(IEnumerable<string> typeAliases)
        {

            var combinedFilter = string.Format("({0})",
                string.Join(" or ",
                    typeAliases.Select(x =>
                        string.Format("ContentTypeAlias eq '{0}'", x)).ToList())
                        );

            _filters.Add(combinedFilter);
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
            
            //Reset the filters to allow multiple Results calls in a single instance
            _resetFilters();

            return Results(sp);        
        }

        private void _resetFilters()
        {
            _filters = new List<string>();
            _orderBy = new List<string>();
            _content = false;
            _media = false;
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

            //Reset the filters to allow multiple Results calls in a single instance
            _resetFilters();

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

            //Reset the filters to allow multiple Results calls in a single instance
            _resetFilters();

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
        public IAzureSearchClient DateRange(string field, DateTime? start, DateTime? end)
        {
            if (start != null || end != null)
            {
                if (start != null && end != null)
                {
                    // is there a better way to format this datetime into a string for azure search?
                    var startDateUtc = (start ?? DateTime.MinValue).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    var endDateUtc = (end ?? DateTime.MaxValue).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    // with a range dates that are greater than or equal to start date, but less than and not equal to end date?
                    _filters.Add(string.Format("({0} ge {1} and {0} lt {2})", field, startDateUtc, endDateUtc));

                }
                //probably don't need these could default to min max values ?
                else if (end != null)
                {
                    //filter before end date
                    var endDateUtc = (end ?? DateTime.MaxValue).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    _filters.Add(string.Format("({0} lt {1})", field, endDateUtc));
                }
                else
                {
                    // filter after start date
                    var startDateUtc = (start ?? DateTime.MinValue).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    _filters.Add(string.Format("({0} ge {1})", field, startDateUtc));
                }
            }
            return this;
        }

        public IAzureSearchClient Filter(string field, string value)
        {
            _filters.Add(string.Format("{0} eq '{1}'", field, value));
            return this;
        }

        public IAzureSearchClient Filter(string field, int value)
        {
            _filters.Add(string.Format("{0} eq {1}", field, value));
            return this;
        }

        public IAzureSearchClient Filter(string field, bool value)
        {
            _filters.Add(string.Format("{0} eq {1}", field, value));
            return this;
        }

        public IAzureSearchClient Any(string field)
        {
            _filters.Add(string.Format("{0}/any()", field));
            return this;
        }
        public IAzureSearchClient Contains(string field, string value)
        {
            _filters.Add(string.Format("{0}/any(x: x eq '{1}')", field, value));
            return this;
        }

        public IList<SuggestResult> Suggest(string value, int count, bool fuzzy = true)
        {
            var client = GetClient();
            var config = GetConfiguration();

            ISearchIndexClient indexClient = client.Indexes.GetClient(config.IndexName);

            SuggestParameters sp = new SuggestParameters()
            {
                UseFuzzyMatching = fuzzy,
                Top = count,
                Filter = "IsContent eq true"
            };

            return indexClient.Documents.Suggest(value, "sg", sp).Results;
        }
    }
}
