using System;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Config;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Core;

namespace Moriyama.AzureSearch.Umbraco.Application.Extensions
{
    public static class SearchFieldsExtensions
    {
        public static void CopyFieldsFromExamineIndexSet(string examineSetName)
        {
            var indexSet = IndexSets.Instance.Sets[examineSetName];
            if (indexSet != null)
            {
                var searchClient = AzureSearchContext.Instance.SearchIndexClient;
                var config = searchClient.GetConfiguration();
                var configuredFields = config.SearchFields?.ToDictionary(f => f.Name);

                foreach (IndexField userField in indexSet.IndexUserFields)
                {
                    SearchField field;
                    if (!configuredFields.TryGetValue(userField.Name, out field))
                    {
                        field = new SearchField();
                        configuredFields.Add(userField.Name, field);
                    }

                    field.Name = userField.Name;
                    field.Type = userField.Type; // TODO: test type mapping
                    field.IsSortable = userField.EnableSorting;
                    field.IsSearchable = true;
                }

                try
                {
                    config.SearchFields = configuredFields.Select(f => f.Value).ToArray();
                    searchClient.SaveConfiguration(config);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static Field ToAzureField(this SearchField field)
        {
            var t = DataType.String;
            switch (field.Type.ToLower())
            {
                case "bool":
                    t = DataType.Boolean;
                    break;
                case "int":
                    t = DataType.Int32;
                    break;
                case "collection":
                    t = DataType.Collection(DataType.String);
                    break;
                case "date":
                    t = DataType.DateTimeOffset;
                    break;
            }

            var f = new Field()
            {
                Name = field.Name,
                Type = t,
                IsFacetable = field.IsFacetable,
                IsFilterable = field.IsFilterable,
                IsSortable = field.IsSortable,
                IsSearchable = field.IsSearchable,
                IsKey = field.IsKey
            };

            return f;
        }

        public static bool IsComputedField(this SearchField field)
        {
            return !string.IsNullOrEmpty(field.ParserType);
        }
    }
}
