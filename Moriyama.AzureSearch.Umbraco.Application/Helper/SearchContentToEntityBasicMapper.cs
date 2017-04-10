using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using System.Collections.Generic;
using Umbraco.Web.Models.ContentEditing;

namespace Moriyama.AzureSearch.Umbraco.Application.Helper
{
    public class SearchContentToEntityBasicMapper
    {
        public static EntityBasic Map(ISearchContent item)
        {
            var e = new EntityBasic();
            e.Name = item.Name;
            e.Id = item.Id;
            e.ParentId = item.ParentId;
            e.Key = new System.Guid();
            e.Icon = item.GetPropertyValue<string>("Icon");
            e.Trashed = item.GetPropertyValue<bool>("Trashed");
            e.Alias = null;

            var path = item.GetPropertyValue<string[]>("Path");

            if(path !=null )
                e.Path = string.Join(",", path);

            return e;
        }
    }
}
