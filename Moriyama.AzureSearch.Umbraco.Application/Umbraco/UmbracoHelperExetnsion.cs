using System.Web;
using Umbraco.Web;

namespace Moriyama.AzureSearch.Umbraco.Application.Umbraco
{
    public static class UmbracoHelperExetnsion
    {
        public static AzureSearchClient SearchContent(this UmbracoHelper helper)
        {
            return new AzureSearchClient(HttpContext.Current.Server.MapPath("/"));
        }
    }
}
