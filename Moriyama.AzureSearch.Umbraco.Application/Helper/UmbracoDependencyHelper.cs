using System.Collections.Generic;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Moriyama.AzureSearch.Umbraco.Application.Helper
{
    public class UmbracoDependencyHelper : IUmbracoDependencyHelper
    {
        public UmbracoHelper GetUmbracoHelper()
        {
            return new UmbracoHelper(UmbracoContext.Current);
        }

        public IContentService GetContentService()
        {
            return ApplicationContext.Current.Services.ContentService;
        }

        public IMediaService GetMediaService()
        {
            return ApplicationContext.Current.Services.MediaService;
        }

        public IMemberService GetMemberService()
        {
            return ApplicationContext.Current.Services.MemberService;
        }

        public IList<T> DatabaseFetch<T>(string s)
        {
            return ApplicationContext.Current.DatabaseContext.Database.Fetch<T>(s);
        }

        public IPublishedContent TypedContent(int id)
        {
            return GetUmbracoHelper().TypedContent(id);
        }

        public IPublishedContent TypedMedia(int id)
        {
            return GetUmbracoHelper().TypedMedia(id);
        }
    }
}
