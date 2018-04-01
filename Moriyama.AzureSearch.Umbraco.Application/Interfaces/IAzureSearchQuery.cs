using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchQuery
    {
        string Term { get; set; }

        bool PopulateContentProperties { get; set; }

        IAzureSearchQuery DocumentType(string typeAlias);

        IAzureSearchQuery DocumentTypes(IEnumerable<string> typeAlias);

        IAzureSearchQuery OrderBy(string fieldName);

        IAzureSearchQuery Content();

        IAzureSearchQuery Media();

        IAzureSearchQuery Member();

        IAzureSearchQuery Page(int page);

        IAzureSearchQuery PageSize(int pageSize);

        IAzureSearchQuery Filter(string field, string value);

        IAzureSearchQuery Filter(string field, string[] values);

        IAzureSearchQuery Filter(string field, int value);

        IAzureSearchQuery Filter(string field, bool value);

        IAzureSearchQuery DateRange(string field, DateTime? start, DateTime? end);

        IAzureSearchQuery Facet(string facet);

        IAzureSearchQuery Facets(string[] facets);

        IAzureSearchQuery Contains(string field, string value);

        IAzureSearchQuery Contains(string field, IEnumerable<string> values);

        IAzureSearchQuery Contains(IEnumerable<string> fields, string value);

        IAzureSearchQuery Contains(IEnumerable<string> fields, IEnumerable<string> values);

        IAzureSearchQuery Any(string field);

        IAzureSearchQuery Highlight(string highlightTag, IEnumerable<string> fields);

        IAzureSearchQuery QueryType(QueryType queryType);

        SearchParameters GetSearchParameters();
    }
}
