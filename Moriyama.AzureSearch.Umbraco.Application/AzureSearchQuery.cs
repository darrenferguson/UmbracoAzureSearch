using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using System;
using System.Linq;
using Microsoft.Azure.Search.Models;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchQuery : IAzureSearchQuery
    {
        #region Fields

        private readonly IList<string> _filters;
        private readonly string _conjunctive = " and ";

        private string _searchTerm = "*";


        private string _scoringProfile = String.Empty;

        private readonly IList<string> _orderBy;
        private readonly IList<string> _facets;

        private IList<string> _highlight;
        private string _highlightTag;

        private bool _content;
        private bool _media;
        private bool _members;

        private int _page;
        private int _pageSize;

        private QueryType _queryType = Microsoft.Azure.Search.Models.QueryType.Simple;

        #endregion

        #region Properties
        public string Term { get; set; }
        public bool PopulateContentProperties { get; set; }

        #endregion

        #region Constructors

        public AzureSearchQuery(string term)
        {
            Term = term;
            PopulateContentProperties = true;

            this._pageSize = 999;
            this._page = 1;
            this._filters = new List<string>();
            this._orderBy = new List<string>();
            this._facets = new List<string>();

            this._highlight = new List<string>();
        }

        public AzureSearchQuery() : this(string.Empty) {}

        #endregion

        #region Methods

        public IAzureSearchQuery DocumentType(string typeAlias)
        {
            _filters.Add(string.Format("ContentTypeAlias eq '{0}'", typeAlias));
            return this;
        }

        public IAzureSearchQuery DocumentTypes(IEnumerable<string> typeAliases)
        {
            var combinedFilter = string.Format("({0})",
                string.Join(" or ",
                    typeAliases.Select(x =>
                        string.Format("ContentTypeAlias eq '{0}'", x)).ToList())
                        );

            _filters.Add(combinedFilter);
            return this;
        }

        public IAzureSearchQuery OrderBy(string fieldName)
        {
            _orderBy.Add(fieldName);
            return this;
        }

        public IAzureSearchQuery Page(int page)
        {
            _page = page;
            return this;
        }

        public IAzureSearchQuery PageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public IAzureSearchQuery Content()
        {
            _content = true;
            return this;
        }

        public IAzureSearchQuery Media()
        {
            _media = true;
            return this;
        }

        public IAzureSearchQuery Member()
        {
            this._members = true;
            return this;
        }

        public IAzureSearchQuery DateRange(string field, DateTime? start, DateTime? end)
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

        public IAzureSearchQuery Filter(string field, string value)
        {
            _filters.Add(string.Format("{0} eq '{1}'", field, value));
            return this;
        }

        public IAzureSearchQuery Filter(string field, int value)
        {
            _filters.Add(string.Format("{0} eq {1}", field, value));
            return this;
        }

        public IAzureSearchQuery Filter(string field, bool value)
        {
            _filters.Add(string.Format("{0} eq {1}", field, value.ToString().ToLower()));
            return this;
        }

        public IAzureSearchQuery Facet(string facet)
        {
            _facets.Add(facet);
            return this;
        }

        public IAzureSearchQuery Facets(string[] facets)
        {
            foreach (var facet in facets)
            {
                _facets.Add(facet);
            }

            return this;
        }

        public IAzureSearchQuery Any(string field)
        {
            this._filters.Add(string.Format("{0}/any()", field));
            return this;
        }

        public IAzureSearchQuery SearchIn(string field, IEnumerable<string> values)
        {
            this._filters.Add($"search.in({field}, '{string.Join(",", values)}')");
            return this;
        }

        public IAzureSearchQuery Highlight(string highlightTag, IEnumerable<string> fields)
        {
            foreach (string field in fields)
            {
                this._highlight.Add(field);
            }

            this._highlightTag = highlightTag;
            return this;
        }

        public IAzureSearchQuery QueryType(QueryType queryType)
        {
            this._queryType = queryType;
            return this;
        }

        public IAzureSearchQuery UseScoringProfile(string name)
        {
            this._scoringProfile = name;
            return this;
        }

        public IAzureSearchQuery Contains(string field, string value)
        {
            this._filters.Add(string.Format("{0}/any(x: x eq '{1}')", field, value));
            return this;
        }

        public IAzureSearchQuery Contains(string field, IEnumerable<string> values)
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
        public IAzureSearchQuery Contains(IEnumerable<string> fields, string value)
        {
            if (fields.Count() > 1)
            {
                var combinedFilter = string.Format("({0})",
                    string.Join(" or ",
                                    fields.Select(x =>
                                    string.Format("{0}/any(x: x eq '{1}')", x, value)).ToList())
                            );

                this._filters.Add(combinedFilter);
            }
            else
            {
                Contains(fields.FirstOrDefault(), value);
            }

            return this;
        }
        public IAzureSearchQuery Contains(IEnumerable<string> fields, IEnumerable<string> values)
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

        public IAzureSearchQuery Filter(string field, string[] values)
        {
            if (values.Count() > 1)
            {
                var combinedFilter = string.Format("({0})",
                    string.Join(" or ",
                                    values.Select(x =>
                                    string.Format("({0} eq '{1}')", field, x)).ToList())
                            );

                this._filters.Add(combinedFilter);
            }
            else
            {
                Filter(field, values.FirstOrDefault());
            }

            return this;
        }

        public SearchParameters GetSearchParameters()
        {
            var searchParameters = new SearchParameters();

            if (this._content)
            {
                this._filters.Add("IsContent eq true");
            }

            if (this._media)
            {
                this._filters.Add("IsMedia eq true");
            }

            if (this._members)
            {
                this._filters.Add("IsMember eq true");
            }

            if (this._filters.Count > 0)
            {
                searchParameters.Filter = string.Join(_conjunctive, _filters);
            }

            if (this._highlight.Any())
            {
                searchParameters.HighlightFields = this._highlight;

                searchParameters.HighlightPreTag = "<" + this._highlightTag + ">";
                searchParameters.HighlightPostTag = "</" + this._highlightTag + ">";
            }


            searchParameters.IncludeTotalResultCount = true;

            searchParameters.Top = this._pageSize;
            searchParameters.Skip = (_page - 1) * this._pageSize;
            searchParameters.OrderBy = this._orderBy;
            searchParameters.Facets = this._facets;

            searchParameters.QueryType = this._queryType;

            if (!string.IsNullOrEmpty(this._scoringProfile))
            {
                searchParameters.ScoringProfile = this._scoringProfile;
            }

            return searchParameters;
        }

        #endregion
    }
}
