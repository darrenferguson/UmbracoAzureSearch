using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using System;
using System.Linq;
using log4net;
using System.Reflection;
using Newtonsoft.Json;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchClient : BaseAzureSearch, IAzureSearchClient
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IList<string> _filters;

        public IList<string> Filters
        {
            get
            {
                return _filters;
            }
            set
            {
                _filters = value;
            }
        }

        private string _conjunctive = " and ";
        private string _searchTerm = "*";
        private IList<string> _orderBy;
        private IList<string> _facets;

        private bool _content;
        private bool _media;

        private int _page;
        private int _pageSize;
        private bool _populateContentProperties = true;

        public AzureSearchClient(string path) : base(path)
        {
            _pageSize = 999;
            _page = 1;
            _filters = new List<string>();
            _orderBy = new List<string>();
            _facets = new List<string>();
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

            if (_content)
            {
                _filters.Add("IsContent eq true");
            }

            if (_media)
            {
                _filters.Add("IsMedia eq true");
            }

            if (_filters.Count > 0)
            {
                sp.Filter = string.Join(_conjunctive, _filters);
            }

            sp.IncludeTotalResultCount = true;

            sp.Top = _pageSize;
            sp.Skip = (_page - 1) * _pageSize;
            sp.OrderBy = _orderBy;
            sp.Facets = _facets;

            return sp;
        }

        public ISearchResult Results()
        {
            var sp = GetSearchParameters();
            return Results(sp);
        }

        private ISearchResult Results(SearchParameters sp)
        {
            var client = GetClient();
            var config = GetConfiguration();
            Log.Info(string.Format("Search info: {0}- Index name = {1}{2}- Base uri = {3}{4}- Uri query string = {5}{6}",
                                    Environment.NewLine,
                                    config.IndexName,
                                    Environment.NewLine,
                                    client.BaseUri,
                                    Environment.NewLine,
                                    sp.ToString(),
                                    Environment.NewLine
                                ));

            ISearchIndexClient indexClient = client.Indexes.GetClient(config.IndexName);
            var response = indexClient.Documents.Search(_searchTerm, sp);

            var results = new Models.SearchResult();

            foreach (var result in response.Results)
            {
                results.Content.Add(FromDocument(result.Document, result.Score));
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

            return results;
        }

        private ISearchContent FromDocument(Document d, double score)
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
                if (property == null && _populateContentProperties)
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

        public IAzureSearchClient Page(int page)
        {
            _page = page;
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
            _filters.Add(string.Format("{0} eq {1}", field, value.ToString().ToLower()));
            return this;
        }

        public IAzureSearchClient Facet(string facet)
        {
            _facets.Add(facet);
            return this;
        }

        public IAzureSearchClient Facets(string[] facets)
        {
            foreach (var facet in facets)
            {
                _facets.Add(facet);
            }

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

        public IAzureSearchClient Contains(string field, IEnumerable<string> values)
        {
            if (values.Count() > 1)
            {
                var combinedFilter = string.Format("({0})",
                    string.Join(" or ",
                                    values.Select(x =>
                                    string.Format("{0}/any(x: x eq '{1}')", field, x)).ToList())
                            );

                _filters.Add(combinedFilter);
            }
            else
            {
                Contains(field, values.FirstOrDefault());
            }

            return this;
        }
        public IAzureSearchClient Contains(IEnumerable<string> fields, string value)
        {
            if (fields.Count() > 1)
            {
                var combinedFilter = string.Format("({0})",
                    string.Join(" or ",
                                    fields.Select(x =>
                                    string.Format("{0}/any(x: x eq '{1}')", x, value)).ToList())
                            );

                _filters.Add(combinedFilter);
            }
            else
            {
                Contains(fields.FirstOrDefault(), value);
            }

            return this;
        }
        public IAzureSearchClient Contains(IEnumerable<string> fields, IEnumerable<string> values)
        {
            if (fields.Count() > 1 && values.Count() > 1)
            {
                // uber filter
                var combinedFilter = string.Format("({0})",
                    string.Join(" or ",
                                    fields.SelectMany(x => values, (x, y) =>
                                    string.Format("{0}/any(x: x eq '{1}')", x, y)).ToList())
                            );

                _filters.Add(combinedFilter);
            }
            else if (fields.Count() > 1)
            {
                Contains(fields, values.FirstOrDefault());
            }
            else if (values.Count() > 1)
            {
                Contains(fields.FirstOrDefault(), values);
            }
            else
            {
                Contains(fields.FirstOrDefault(), values.FirstOrDefault());
            }

            return this;
        }

        public IAzureSearchClient Filter(string field, string[] values)
        {
            if (values.Count() > 1)
            {
                var combinedFilter = string.Format("({0})",
                    string.Join(" or ",
                                    values.Select(x =>
                                    string.Format("({0} eq '{1}')", field, x)).ToList())
                            );

                _filters.Add(combinedFilter);
            }
            else
            {
                Filter(field, values.FirstOrDefault());
            }

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
