using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Moriyama.AzureSearch.Umbraco.Application.Helper;
using Umbraco.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Moriyama.AzureSearch.Umbraco.Application.Controllers.BackOffice
{
    public class BackOfficeAzureSearchApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public IEnumerable<EntityTypeSearchResult> Search(string query)
        {
            var allowedSections = Security.CurrentUser.AllowedSections.ToArray();

            if (string.IsNullOrEmpty(query))
                return Enumerable.Empty<EntityTypeSearchResult>();

            var result = new List<EntityTypeSearchResult>();

            var client = AzureSearchContext.Instance.GetSearchClient();
            // if the search term contains a space this will be transformed to %20 and no search results returned
            // so lets decode the query term to turn it back into a proper space
            // will this mess up any other Url encoded terms? or fix them too?
            query = HttpUtility.UrlDecode(query);
            var searchResults = client.Term(query + "*").Results();

            if (allowedSections.InvariantContains(Constants.Applications.Content))
            {

                var entities = new List<EntityBasic>();

                foreach (var searchResult in searchResults.Content)
                {
                    if (searchResult.IsContent)
                    {
                        var entity = SearchContentToEntityBasicMapper.Map(searchResult);
                        entities.Add(entity);
                    }
                }

                result.Add(new EntityTypeSearchResult
                {
                    Results = entities,
                    EntityType = UmbracoEntityTypes.Document.ToString()
                });
            }

            if (allowedSections.InvariantContains(Constants.Applications.Media))
            {
                var entities = new List<EntityBasic>();
                foreach (var searchResult in searchResults.Content)
                {
                    if (searchResult.IsMedia)
                    {
                        var entity = SearchContentToEntityBasicMapper.Map(searchResult);
                        entities.Add(entity);
                    }
                }

                result.Add(new EntityTypeSearchResult
                {
                    Results = entities,
                    EntityType = UmbracoEntityTypes.Media.ToString()
                });
            }

            if (allowedSections.InvariantContains(Constants.Applications.Members))
            {
                var entities = new List<EntityBasic>();
                foreach (var searchResult in searchResults.Content)
                {
                    if (searchResult.IsMember)
                    {
                        var entity = SearchContentToEntityBasicMapper.Map(searchResult);
                        entities.Add(entity);
                    }
                }

                result.Add(new EntityTypeSearchResult
                {
                    Results = entities,
                    EntityType = UmbracoEntityTypes.Member.ToString()
                });
            }

            return result;
        }
    }
}
