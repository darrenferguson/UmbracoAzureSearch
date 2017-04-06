using System.Web.Http;
using Umbraco.Web.Editors;
using Umbraco.Web.WebApi;

namespace Moriyama.AzureSearch.Umbraco.Application.Controllers.BackOffice
{
    public class BackOfficeAzureSearchApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public object Search(string query)
        {
            return new object();
        }
    }
}
