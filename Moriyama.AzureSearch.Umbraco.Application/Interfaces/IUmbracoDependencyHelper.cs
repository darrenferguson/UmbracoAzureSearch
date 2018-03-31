using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Moriyama.AzureSearch.Umbraco.Application.Interfaces
{
    public interface IUmbracoDependencyHelper
    {

        UmbracoHelper GetUmbracoHelper();

        IContentService GetContentService();
        IMediaService GetMediaService();

        IMemberService GetMemberService();
        
        IList<T> DatabaseFetch<T>(string s);
            
        IPublishedContent TypedContent(int id);

        IPublishedContent TypedMedia(int id);
    }
}
