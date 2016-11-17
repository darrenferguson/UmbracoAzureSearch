﻿using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IAzureSearchClient
    {
        IEnumerable<ISearchContent> Results();
        IEnumerable<ISearchContent> Results(int page);

        IAzureSearchClient Term(string query);
        IAzureSearchClient DocumentType(string typeAlias);
        IAzureSearchClient OrderBy(string fieldName);

        IAzureSearchClient Content();
        IAzureSearchClient Media();

        IAzureSearchClient PageSize(int pageSize);
        IAzureSearchClient PopulateContentProperties(bool populate);

    }
}