using System;
using System.Collections;
using System.Collections.Generic;
using Examine;
using System.Linq;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using SearchResult = Examine.SearchResult;

namespace Moriyama.AzureSearch.Umbraco.Application.Examine
{
    public class AzureExamineSearchResults : ISearchResults
    {
        public AzureExamineSearchResults(ISearchResult results)
        {
            _azureResults = results;

            if (results != null) 
                TotalItemCount = results.Count;
        }

        private readonly ISearchResult _azureResults;

        public IEnumerator<SearchResult> GetEnumerator()
        {
            using (var iterator = _azureResults?.Content?.GetEnumerator())
            {
                while (iterator != null && iterator.MoveNext())
                {
                    yield return Convert(iterator.Current);
                }
            }
        }

        private SearchResult Convert(ISearchContent azureResult)
        {
            if (azureResult == null) return null;

            var result = new SearchResult
            {
                Id = azureResult.Id,
                Score = (float) azureResult.Score,
            };

            if (result.Fields == null) return result;

            var indexType = "content";
            var publishedContentType = PublishedItemType.Content;

            if (azureResult.IsMedia)
            {
                indexType = "media";
                result.Fields.Add("umbracoFile", azureResult.Url);

                publishedContentType = PublishedItemType.Media;
            }

            if (azureResult.IsMember)
            {
                indexType = "member";
                publishedContentType = PublishedItemType.Member;
            }

            result.Fields.Add("__IndexType", indexType);
            result.Fields.Add("__NodeId", azureResult.Id.ToString());
            result.Fields.Add("__Path", $"-{azureResult.SearchablePath}");
            result.Fields.Add("__NodeTypeAlias", azureResult.ContentTypeAlias?.ToLower());
            result.Fields.Add("__Key", azureResult.Key);
            result.Fields.Add("__Match", (azureResult.Properties?["__match"] ?? "").ToString());
            result.Fields.Add("id", azureResult.Id.ToString());
            result.Fields.Add("key", azureResult.Key);
            result.Fields.Add("parentID", azureResult.ParentId.ToString());
            result.Fields.Add("level", azureResult.Level.ToString());
            result.Fields.Add("creatorID", azureResult.CreatorId.ToString());
            result.Fields.Add("creatorName", azureResult.CreatorName);
            result.Fields.Add("writerID", azureResult.WriterId.ToString());
            result.Fields.Add("writerName", azureResult.CreatorName);
            result.Fields.Add("template", azureResult.Template.IsNullOrWhiteSpace() ? "0" : azureResult.Template);
            result.Fields.Add("urlName", "");
            result.Fields.Add("sortOrder", azureResult.SortOrder.ToString());
            result.Fields.Add("createDate", azureResult.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
            result.Fields.Add("updateDate", azureResult.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss"));
            result.Fields.Add("path", $"-{azureResult.SearchablePath}");
            result.Fields.Add("nodeType", azureResult.ContentTypeId.ToString());

            result.Fields.Add("nodeName", azureResult.Name);

            if (azureResult.Properties == null) return result;

            // only add valid properties for this content type
            var contentType = PublishedContentType.Get(publishedContentType, azureResult.ContentTypeAlias);
            var validProperties = contentType.PropertyTypes.Select(p => p.PropertyTypeAlias).ToList();
            
            foreach (var prop in azureResult.Properties)
            {
                if (prop.Key == null || prop.Value == null) continue;

                if (validProperties.Contains(prop.Key))
                {
                    result.Fields[prop.Key] = GetPropertyString(prop.Value);
                }
            }

            var icon = "";
            object iconObj = null;
            if (azureResult.Properties?.TryGetValue("Icon", out iconObj) == true)
            {
                icon = iconObj.ToString();
            }

            result.Fields.Add("__Icon", icon);

            return result;
        }

        private static string GetPropertyString(object value)
        {
            if (value == null) return string.Empty;

            var type = value.GetType();
            if (type == typeof(string) || !type.IsEnumerable())
            {
                return value.ToString();
            }

            switch (value)
            {
                case IEnumerable<object> enumerable:
                    var values = enumerable.Select(i => i?.ToString());
                    return string.Join(", ", values);

                default:
                    return JsonConvert.SerializeObject(value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<SearchResult> Skip(int skip)
        {
            using (var enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        public int TotalItemCount
        {
            get;
        }
    }
}

