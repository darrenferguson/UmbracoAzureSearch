using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
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
            e.Icon = "icon-windows color-blue";
            e.Trashed = false;
            e.Alias = null;
            e.Path = "-1,1063,2652,2662,2921,23153";
            
            return e;
        }

    }
}
