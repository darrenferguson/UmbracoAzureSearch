using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Extensions
{
    public static class SearchFieldsExtensions
    {
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
