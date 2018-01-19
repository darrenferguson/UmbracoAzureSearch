using System;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
    public abstract class BaseContent
    {

        public string Name { get; set; }
        public string UrlName { get; set; }
        
        public bool Published { get; set; }

        public int SortOrder { get; set; }
        public int Level { get; set; }
        
        public int ParentId { get; set; }

        public DateTime UpdateDate { get; set; }
        public bool Trashed { get; set; }
        public int WriterId { get; set; }
        public string WriterName {get; set;}
        public string Template { get; set; }

        public int ContentTypeId { get; set; }
        public string ContentTypeAlias { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreatorId { get; set; }

        public string CreatorName {get; set;}
    }
}
