using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Moriyama.AzureSearch.Umbraco.Application.Helper;
using Umbraco.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using System.Web;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Umbraco.Web.Search;
using Umbraco.Core.Models;
using Umbraco.Web.Trees;
using System;
using Umbraco.Core.Services;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Azure.Search;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using StackExchange.Profiling;
using File = System.IO.File;

namespace Moriyama.AzureSearch.Umbraco.Application.Controllers.BackOffice
{
    public class BackOfficeAzureSearchApiController : UmbracoAuthorizedJsonController
    {
        private static readonly ConcurrentDictionary<Type, TreeAttribute> TreeAttributeCache = new ConcurrentDictionary<Type, TreeAttribute>();
        private const int NumberOfItemsPerSection = 10;

        [HttpGet]
        public IDictionary<string, TreeSearchResult> Search(string query)
        {
            IDictionary<string, TreeSearchResult> result = GetTreeSearchResultStructure();

            if (string.IsNullOrEmpty(query))
            {
                return result;
            }

            IAzureSearchClient client = AzureSearchContext.Instance.GetSearchClient();

            // if the search term contains a space this will be transformed to %20 and no search results returned
            // so lets decode the query term to turn it back into a proper space
            // will this mess up any other Url encoded terms? or fix them too?
            query = HttpUtility.UrlDecode(query);
          

            ISearchResult searchResults = client.Term(query + "*").Results();

            if (result.Keys.Any(x => x.Equals(Constants.Applications.Content, StringComparison.CurrentCultureIgnoreCase)))
            {
                List<SearchResultItem> entities = new List<SearchResultItem>();

                foreach (ISearchContent searchResult in searchResults.Content.Where(c => c.IsContent).Take(NumberOfItemsPerSection))
                {
                    var entity = SearchContentToEntityBasicMapper.Map(searchResult);
                    entities.Add(entity);
                }

                result.First(x => x.Key.Equals(Constants.Applications.Content, StringComparison.CurrentCultureIgnoreCase))
                      .Value.Results = entities;
            }

            if (result.Keys.Any(x => x.Equals(Constants.Applications.Media, StringComparison.CurrentCultureIgnoreCase)))
            {
                List<SearchResultItem> entities = new List<SearchResultItem>();

                foreach (ISearchContent searchResult in searchResults.Content.Where(c => c.IsMedia).Take(NumberOfItemsPerSection))
                {
                    var entity = SearchContentToEntityBasicMapper.Map(searchResult);
                    entities.Add(entity);
                }

                result.First(x => x.Key.Equals(Constants.Applications.Media, StringComparison.CurrentCultureIgnoreCase))
                      .Value.Results = entities;
            }

            if (result.Keys.Any(x => x.Equals(Constants.Applications.Members, StringComparison.CurrentCultureIgnoreCase)))
            {
                List<SearchResultItem> entities = new List<SearchResultItem>();
                ApplicationTree tree = Services.ApplicationTreeService.GetByAlias(Constants.Applications.Members);

                foreach (ISearchContent searchResult in searchResults.Content.Where(c => c.IsMember).Take(NumberOfItemsPerSection))
                {
                    var entity = SearchContentToEntityBasicMapper.Map(searchResult);
                    entities.Add(entity);
                }

                result.First(x => x.Key.Equals(Constants.Applications.Members, StringComparison.CurrentCultureIgnoreCase))
                      .Value.Results = entities;
            }

            return result;
        }

        public SearchServiceClient GetClient(AzureSearchConfig config)
        {
            
            var profiler = MiniProfiler.Current;
            using (profiler.Step($"Calling GetClient"))
            {
                var serviceClient = new SearchServiceClient(config.SearchServiceName,  new SearchCredentials(config.SearchServiceAdminApiKey));
                return serviceClient;
            }
        }

        private IDictionary<string, TreeSearchResult> GetTreeSearchResultStructure()
        {
            Dictionary<string, TreeSearchResult> result = new Dictionary<string, TreeSearchResult>();
            string[] allowedSections = Security.CurrentUser.AllowedSections.ToArray();
            IReadOnlyDictionary<string, SearchableApplicationTree> searchableTrees = SearchableTreeResolver.Current.GetSearchableTrees();

            foreach (var searchableTree in searchableTrees)
            {
                if (allowedSections.Contains(searchableTree.Value.AppAlias))
                {
                    ApplicationTree tree = Services.ApplicationTreeService.GetByAlias(searchableTree.Key);
                    if (tree == null) continue; //shouldn't occur

                    SearchableTreeAttribute searchableTreeAttribute = searchableTree.Value.SearchableTree.GetType().GetCustomAttribute<SearchableTreeAttribute>(false);
                    TreeAttribute treeAttribute = GetTreeAttribute(tree);

                    result[GetRootNodeDisplayName(treeAttribute, Services.TextService)] = new TreeSearchResult
                    {
                        Results = Enumerable.Empty<SearchResultItem>(),
                        TreeAlias = searchableTree.Key,
                        AppAlias = searchableTree.Value.AppAlias,
                        JsFormatterService = searchableTreeAttribute == null ? "" : searchableTreeAttribute.ServiceName,
                        JsFormatterMethod = searchableTreeAttribute == null ? "" : searchableTreeAttribute.MethodName
                    };
                }
            }

            return result;
        }

        internal static TreeAttribute GetTreeAttribute(ApplicationTree tree)
        {
            return GetTreeAttribute(tree.GetRuntimeType());
        }

        internal static TreeAttribute GetTreeAttribute(Type treeControllerType)
        {
            return TreeAttributeCache.GetOrAdd(treeControllerType, type =>
            {
                //Locate the tree attribute
                var treeAttributes = type
                    .GetCustomAttributes<TreeAttribute>(false)
                    .ToArray();

                if (treeAttributes.Length == 0)
                {
                    throw new InvalidOperationException("The Tree controller is missing the " + typeof(TreeAttribute).FullName + " attribute");
                }

                //assign the properties of this object to those of the metadata attribute
                return treeAttributes[0];
            });
        }

        internal static string GetRootNodeDisplayName(TreeAttribute attribute, ILocalizedTextService textService)
        {
            var label = $"[{attribute.Alias}]";

            // try to look up a the localized tree header matching the tree alias
            var localizedLabel = textService.Localize("treeHeaders/" + attribute.Alias);

            // if the localizedLabel returns [alias] then return the title attribute from the trees.config file, if it's defined
            if (localizedLabel != null && localizedLabel.Equals(label, StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(attribute.Title) == false)
                    label = attribute.Title;
            }
            else
            {
                // the localizedLabel translated into something that's not just [alias], so use the translation
                label = localizedLabel;
            }

            return label;
        }
    }
}
