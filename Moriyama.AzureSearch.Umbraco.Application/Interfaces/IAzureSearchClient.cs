using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchClient
    {
        IList<string> Filters { get; set; }

        ISearchResult Results();
        ISearchResult Results(string scoringProfile);
        IAzureSearchClient Term(string query);
        IAzureSearchClient DocumentType(string typeAlias);
        IAzureSearchClient DocumentTypes(IEnumerable<string> typeAlias);
        IAzureSearchClient OrderBy(string fieldName);

        IAzureSearchClient Content();
        IAzureSearchClient Media();
        IAzureSearchClient Member();
        IAzureSearchClient Page(int page);
        IAzureSearchClient PageSize(int pageSize);
        IAzureSearchClient PopulateContentProperties(bool populate);

        IAzureSearchClient Filter(string field, string value);
        IAzureSearchClient Filter(string field, string[] values);
        IAzureSearchClient Filter(string field, int value);
        IAzureSearchClient Filter(string field, int[] values);
        IAzureSearchClient Filter(string field, bool value);

        IAzureSearchClient DateRange(string field, DateTime? start, DateTime? end);

        IAzureSearchClient Facet(string facet);
        IAzureSearchClient Facets(string[] facets);

        IAzureSearchClient Contains(string field, string value);
        IAzureSearchClient Contains(string field, IEnumerable<string> values);
        IAzureSearchClient Contains(IEnumerable<string> fields, string value);
        IAzureSearchClient Contains(IEnumerable<string> fields, IEnumerable<string> values);

        IAzureSearchClient Any(string field);

        IList<SuggestResult> Suggest(string value, int count, bool fuzzy = true);

        IAzureSearchClient SearchMode(SearchMode searchMode);
    }
}
