using System;
using System.Collections;
using System.Collections.Generic;
using Examine;
using System.Linq;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Newtonsoft.Json;
using Umbraco.Core;
using SearchResult = Examine.SearchResult;

namespace Moriyama.AzureSearch.Umbraco.Application.Examine
{
    public class AzureExamineSearchResults : ISearchResults
    {
        private readonly ISearchResult _azureResults;

        public AzureExamineSearchResults(ISearchResult results)
        {
            this._azureResults = results;

            this.TotalItemCount = results.Count;
        }

        
        public IEnumerator<SearchResult> GetEnumerator()
        {
            using (var iterator = this._azureResults?.Content?.GetEnumerator())
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
                DocId = azureResult.Id,
                Score = (float) azureResult.Score,
            };

            if (result.Fields == null)
                return result;

            string indexType = "content";
            if (azureResult.IsMedia)
            {
                indexType = "media";
            }

            if (azureResult.IsMember)
            {
                indexType = "member";
            }

            result.Fields.Add("__IndexType", indexType);
            result.Fields.Add("__NodeId", azureResult.Id.ToString());

            // TODO: Ask Tom?
            // result.Fields.Add("__Path", $"-{azureResult.SearchablePath}");

            result.Fields.Add("__NodeTypeAlias", azureResult.ContentTypeAlias?.ToLower());
            result.Fields.Add("__Key", azureResult.Key);
            result.Fields.Add("id", azureResult.Id.ToString());
            result.Fields.Add("nodeName", azureResult.Name);
            result.Fields.Add("createDate", azureResult.CreateDate.ToString("yyyyMMddHHmmsss"));

            if (azureResult.Properties == null) return result;

            foreach (var prop in azureResult.Properties)
            {
                if (prop.Key == null || prop.Value == null) continue;

                result.Fields[prop.Key] = GetPropertyString(prop.Value);
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

