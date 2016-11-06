using AutoMapper;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Moriyama.AzureSearch.Umbraco.Application.Umbraco
{
    public class CustomApplicationEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Mapper.CreateMap<Field, SearchField>().ForMember(dest => dest.Type,
               opts => opts.MapFrom(
                   src => src.Type.ToString()
              ));

            Mapper.CreateMap<Index, SearchIndex>();

            ContentService.Published += ContentServicePublished;
            MediaService.Saved += MediaServiceSaved;
        }

        private void MediaServiceSaved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            var azureSearchServiceClient = new AzureSearchServiceClient(HttpContext.Current.Server.MapPath("/"));
            foreach(var entity in e.SavedEntities)
            {
                azureSearchServiceClient.ReIndexContent(entity);
            }   
        }

        private void ContentServicePublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            var azureSearchServiceClient = new AzureSearchServiceClient(HttpContext.Current.Server.MapPath("/"));
            foreach (var entity in e.PublishedEntities)
            {
                azureSearchServiceClient.ReIndexContent(entity);
            }
        }
    }
}
