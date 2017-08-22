using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Extensions
{
    public static class SearchFieldsExtensions
    {
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
                    t = DataType.Collection(DataType.DateTimeOffset);
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
    }
}
