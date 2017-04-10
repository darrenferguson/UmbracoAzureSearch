# UmbracoAzureSearch
UmbracoAzureSearch allows you to completely replace Examine from your Umbraco website and replace it with [Azure Search](https://azure.microsoft.com/en-gb/services/search/).

## Getting Started
The first thing required to get your UmbracoAzureSearch service up and running will be to create an Azure search service.

[Steps to setting up an Azure Search instance](https://docs.microsoft.com/en-us/azure/search/search-create-service-portal)

### Configuring the API
Inside the `~/config/AzureSearch.config` you will want to populate the `SearchServiceName`, the `SearchServiceAdminApiKey` (which you can find in your Azure Search instance, under Settings > Keys), and lastly the `IndexName` of which you have named your search index.

    {
        "SearchServiceName" : "my-umbraco-search",
        "SearchServiceAdminApiKey" : "XXXXXXXXXXXXXXXXXXXXXX",
        "IndexName": "MyUmbracoIndex"
    }
    
### AzureSearchClient
The AzureSearchClient provides a fluent interface to allow you to build and execute your Azure Search queries against your newly defined index.

    public class SearchController : RenderMvcController 
    {
        private readonly IAzureSearchClient _search;
        
        public SearchController()
        {
            this._search = new AzureSearchClient(HttpContext.Current.Server.MapPath("/"));
        }
    }
    
### Content Search
    public ISearchResult GetContent(string searchTerm)
    {
        return this._search.Content().Term(searchTerm).Results();
    }
    
### Media Search
Media will also return an `IAzureSearchClient` so will allow you to use any of the following fluent methods for filtering.

    public ISearchResult GetMedia()
    {
        return this_.search.Media().Results();
    }
    
### Filter by DocumentType
Filter by a single DocumentType

    public ISearchResult GetContent(string searchTerm)
    {
        return this._search.Content().Term(searchTerm).DocumentType("newsArticle").Results();
    }
    
Filter by multiple DocumentTypes

    public ISearchResult GetContent(string searchTerm)
    {
        return this._search.Content().Term(searchTerm).DocumentTypes(new { "newsArticle", "contentPage" }).Results();
    }
    
### Order Results
Order by a property alias

    public ISearchResult GetContent(string searchTerm)
    {
        return this._search.Content().Term(searchTerm).OrderBy("publicationDate").Results();
    }
    
### Pagination
Paginate results

    public ISearchResult GetContent(string searchTerm, int page)
    {
        return this._search.Content().Term(searchTerm).PageSize(10).Results(page);
    }
    
### Search Root
Define the base node from which all of your search queries originate.

    public ISearchResult GetContent(string searchTerm)
    {
        return this._search.Content().Term(searchTerm).SearchRoot(1).Results();
    }
    
### Filtering
Filtering allows overloads for `string`,`int` and `bool` to filter on a property value. Properties must include the `IsFilterable: True` flag within the index configuration.
    
Config:

    {
        "Name":"published",
        "Type":"bool",
        "IsFilterable":"True"
    },
    
Code:

    public ISearchResult GetContent(string searchTerm)
    {
        return this._search.Content().Term(searchTerm).Filter("published", true).Results();
    }
    
### Facets
Facets are configured from within the index configuration by assigning the `IsFacetable: True` flag on a property.

Config:

    {
        "Name":"year",
        "Type":"string",
        "IsFacetable":"True"
    },
    
Code:

    public ISearchResult GetFacets()
    {
        return this._search.Content().Facet("year").Results();
    }

## Searching on a Custom Property
In order to search on a custom property, you must ensure that the `IsSearchable: True` flag is set on the property. This will allow us to use the `Contains()` method.

Config:

    {
        "Name":"title",
        "Type":"string",
        "IsSearchable":"True"
    },
    
Code:

    public ISearchResult GetContent(string searchTerm)
    {
        return this._search.Content().Contains("title", searchTerm).Results();
    }
    
## Autocomplete Suggestions
In order to enable autocomplete functionality in Azure Search, you must first add a suggester to your index. The standard suggester will allow you to chose which of your indexed fields the autocomplete search will be based on.

    "Suggesters": [
        {
            "Name": "sg",
            "SearchFields": [
                "Name",
                "title"
            ]
        }
    ]
    
After this is defined and your index is reconstructed, we can start autocomplete based on search terms.

    public IEnumerable<string> Autocomplete(string searchTerm)
    {
        IList<SuggestResult> result = this._searchService.Suggest(searchTerm, 10);
        return result.Select(x => x.Text).Distinct().ToList();
    }