using Umbraco.Web;

namespace Moriyama.AzureSearch.Umbraco.Application.Umbraco
{
    public static class UmbracoHelperExetnsion
    {
        public static AzureSearchClient SearchContent(this UmbracoHelper helper)
        {
            return (AzureSearchClient)AzureSearchContext.Instance.GetSearchClient();
        }
    }
}
