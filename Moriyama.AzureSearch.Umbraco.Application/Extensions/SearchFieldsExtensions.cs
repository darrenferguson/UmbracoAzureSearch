using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
﻿using System;
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
        //public static void CopyFieldsFromExamineIndexSet(string examineSetName)
        //{
        //    var indexSet = IndexSets.Instance.Sets[examineSetName];
        //    if (indexSet != null)
        //    {
        //        var searchClient = AzureSearchContext.Instance.SearchIndexClient;
        //        var config = searchClient.GetConfiguration();
        //        var configuredFields = config.Fields?.ToDictionary(f => f.Name);

        //        foreach (IndexField userField in indexSet.IndexUserFields)
        //        {
        //            SearchField field;
        //            if (!configuredFields.TryGetValue(userField.Name, out field))
        //            {
        //                field = new SearchField();
        //                configuredFields.Add(userField.Name, field);
        //            }

        //            field.Name = userField.Name;
        //            //field.FieldType = userField.Type; // TODO: test type mapping
        //            field.IsSortable = userField.EnableSorting;
        //            field.IsSearchable = true;
        //        }


        //        config.Fields = configuredFields.Select(f => f.Value).ToArray();
        //    }
        //}

        public static Field ToAzureField(this SearchField field)
        {
            var t = DataType.String;
            switch (field.FieldType)
            {
                case FieldType.Bool:
                    t = DataType.Boolean;
                    break;
                case FieldType.Int:
                    t = DataType.Int32;
                    break;
                case FieldType.Collection:
                    t = DataType.Collection(DataType.String);
                    break;
                case FieldType.Date:
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
                IsKey = field.IsKey,
                Analyzer = !string.IsNullOrWhiteSpace(field.Analyzer) ? AnalyzerName.Create(field.Analyzer) : null
            };

            return f;
        }

        public static bool IsComputedField(this SearchField field)
        {
            return !string.IsNullOrEmpty(field.ParserType);
        }
    }
}
