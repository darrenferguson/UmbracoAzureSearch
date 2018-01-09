﻿using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Umbraco.Web;

namespace Moriyama.AzureSearch.Umbraco.Application.Extensions
{
    public static class UmbracoHelperExtensions
    {
        public static IAzureSearchQuery Query(this UmbracoHelper helper, string term = "")
        {
            return new AzureSearchQuery(term);
        }

        public static ISearchResult Results(this UmbracoHelper helper, IAzureSearchQuery query)
        {
            return AzureSearchContext.Instance.SearchClient.Results(query);
        }
    }
}
