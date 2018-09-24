using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Moriyama.AzureSearch.Umbraco.Application.Helper;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Umbraco.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Moriyama.AzureSearch.Umbraco.Application.Controllers.BackOffice
{
    public class BackOfficeAzureSearchApiController : UmbracoAuthorizedJsonController
    {

        private readonly IAzureSearchClient _azureSearchClient;

        public BackOfficeAzureSearchApiController()
        {
            this._azureSearchClient = AzureSearchContext.Instance.SearchClient;
        }

        [HttpGet]
        public IEnumerable<TreeSearchResult> Search(string query)
        {
            string[] allowedSections = Security.CurrentUser.AllowedSections.ToArray();

            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<TreeSearchResult>();

            // if the search term contains a space this will be transformed to %20 and no search results returned
            // so lets decode the query term to turn it back into a proper space
            // will this mess up any other Url encoded terms? or fix them too?
            query = HttpUtility.UrlDecode(query);
            AzureSearchQuery azureSearchQuery = new AzureSearchQuery(query + "*");

            ISearchResult searchResults = this._azureSearchClient.Results(azureSearchQuery);

            IList<SearchResultItem> contentEntities = new List<SearchResultItem>();
            IList<SearchResultItem> mediaEntities = new List<SearchResultItem>();
            IList<SearchResultItem> memberEntities = new List<SearchResultItem>();

            foreach (ISearchContent searchResult in searchResults.Content)
            {
                SearchResultItem entity = SearchContentToSearchResultItemMapper.Map(searchResult);
                if (searchResult.IsContent && allowedSections.InvariantContains(Constants.Applications.Content))
                {
                    contentEntities.Add(entity);
                } else if (searchResult.IsMedia && allowedSections.InvariantContains(Constants.Applications.Media))
                {
                    mediaEntities.Add(entity);
                } else if (searchResult.IsMember && allowedSections.InvariantContains(Constants.Applications.Members))
                {
                    memberEntities.Add(entity);
                }
            }

            return new List<TreeSearchResult>
            {
                new TreeSearchResult
                {
                    Results = contentEntities,                                   
                    TreeAlias = "content",
                    AppAlias = "content"

                },
                new TreeSearchResult
                {
                    Results = mediaEntities,
                    TreeAlias = "media",
                    AppAlias = "media"
                },
                new TreeSearchResult
                {
                    Results = memberEntities,
                    TreeAlias = "member",
                    AppAlias = "member"
                }
            };
        }
    }
}
