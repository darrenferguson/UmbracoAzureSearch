using System.Collections.Generic;
using System.Linq;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Moriyama.AzureSearch.Umbraco.Application;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.ExampleSimpleData
{
    public class AzureSearchSimpleDataSetExampleService : IAzureSearchSimpleDataService
    {

        public IAzureSearchSimpleDataSet Get(IContent content)
        {
            return GettBatchData(new int[] { content.Id }).FirstOrDefault();
        }

        public List<int> GetAllIds()
        {
            List<int> contentIds;

            using (var db = new UmbracoDatabase("umbracoDbDSN"))
            {
                contentIds = db.Fetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = 'C66BA18E-EAF3-4CFF-8A22-41B16D66A972'");
            }

            return contentIds;
        }

        public IEnumerable<IAzureSearchSimpleDataSet> GettBatchData(int[] ids)
        {
            var contents = UmbracoContext.Current.Application.Services.ContentService.GetByIds(ids);

            return contents.Select(x =>
            {
                return new AzureSearchSimpleDataSet()
                {
                    Id = x.Id,
                    Key = x.Key.ToString(),
                    RowData = new Dictionary<string, ISearchValue>()
                    {
                        { "name", new SearchValue() { String = x.Name } }
                    }
                };
            });
        }
    }
}
