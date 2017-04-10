using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public class BackOfficeSearchResult
    {
        public string type { get; set; }
        public IEnumerable<BackOfficeSearchResultItem> results { get; set; }
    }

    public class BackOfficeSearchResultItem
    {
        public string name { get; set; }
        public int id { get; set; }
        public string icon { get; set; }

        public bool trashed { get; set; }

        public Guid key { get; set; }

        public int parentId { get; set; }

        public string alias { get; set; }

        public string path { get; set; }

        public BackOfficeSearchMeta metaData { get; set; }

        // name":"Antonio Da Ros","id":23153,"icon":"icon-windows color-blue","trashed":false,
        // "key":"00000000-0000-0000-0000-000000000000","parentId":2921,"alias":null,
        // "path":"-1,1063,2652,2662,2921,23153",
        // "metaData":{"contentType":"brandtag","Url":"/categories/decorative-art/glassware/antonio-da-ros/"}

    }

    public class BackOfficeSearchMeta
    {
        public string contentType { get; set; }
        public string Url { get; set; }
    }
}
