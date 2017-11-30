using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface ISearchContent
    {
        int Id { get; set; }

        double Score { get; set; }

        string Name { get; set; }
        string Key { get; set; }
        bool Published { get; set; }
        string Url { get; set; }

        bool IsContent { get; set; }
        bool IsMedia { get; set; }
        bool IsMember { get; set; }

        int SortOrder { get; set; }
        int Level { get; set; }

        string[] Path { get; set; }

        int ParentId { get; set; }

        DateTime UpdateDate { get; set; }
        bool Trashed { get; set; }
        int WriterId { get; set; }
        string Template { get; set; }

        int ContentTypeId { get; set; }
        string ContentTypeAlias { get; set; }
        DateTime CreateDate { get; set; }
        int CreatorId { get; set; }

        IDictionary<string, object> Properties { get; set; }
    }
}
