using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;

//Moriyama.AzureSearch.Umbraco.Application.AzureSearchSimpleDataSetExampleService

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchSimpleDataSetExampleService : IAzureSearchSimpleDataService
    {
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
                    RowData = new Dictionary<string, ISearchValue>()
                    {
                        { "name", new SearchValue() { String = x.Name } }
                    }
                };
            });
        }
    }
}
