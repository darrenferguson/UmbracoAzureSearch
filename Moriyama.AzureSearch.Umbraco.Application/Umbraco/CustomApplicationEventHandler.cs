using AutoMapper;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
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

            ContentService.Saved += ContentServiceSaved;
            ContentService.Published += ContentServicePublished;
            ContentService.Trashed += ContentServiceTrashed;
            ContentService.Deleted += ContentServiceDeleted;
            ContentService.EmptiedRecycleBin += ContentServiceEmptiedRecycleBin;

            MediaService.Saved += MediaServiceSaved;
            MediaService.Trashed += MediaServiceTrashed;
            MediaService.Deleted += MediaServiceDeleted;

            MemberService.Saved += MemberServiceSaved;
            MemberService.Deleted += MemberServiceDeleted;
        }

        private void ContentServiceEmptiedRecycleBin(IContentService sender, RecycleBinEventArgs e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;

            foreach (var id in e.Ids)
            {
                azureSearchServiceClient.Delete(id);
            }
        }

        private void MediaServiceDeleted(IMediaService sender, DeleteEventArgs<IMedia> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;

            foreach (var entity in e.DeletedEntities)
            {
                azureSearchServiceClient.Delete(entity.Id);
            }
        }

        private void ContentServiceDeleted(IContentService sender, DeleteEventArgs<IContent> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;

            foreach (var entity in e.DeletedEntities)
            {
                azureSearchServiceClient.Delete(entity.Id);
            }
        }

        private void ContentServiceSaved(IContentService sender, SaveEventArgs<IContent> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;

            foreach (var entity in e.SavedEntities)
            {
                azureSearchServiceClient.ReIndexContent(entity);
            }
        }

        private void MemberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;

            foreach (var entity in e.DeletedEntities)
            {
                azureSearchServiceClient.Delete(entity.Id);
            }
        }

        private void ContentServiceTrashed(IContentService sender, MoveEventArgs<IContent> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;
            foreach (var item in e.MoveInfoCollection)
            {
                azureSearchServiceClient.ReIndexContent(item.Entity);
            }
        }

        private void MediaServiceTrashed(IMediaService sender, MoveEventArgs<IMedia> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;
            foreach (var item in e.MoveInfoCollection)
            {
                azureSearchServiceClient.ReIndexContent(item.Entity);
            }
        }

        private void MemberServiceSaved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;
            foreach (var entity in e.SavedEntities)
            {
                azureSearchServiceClient.ReIndexMember(entity);
            }
        }

        private void MediaServiceSaved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;
            foreach (var entity in e.SavedEntities)
            {
                azureSearchServiceClient.ReIndexContent(entity);
            }   
        }

        private void ContentServicePublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            var azureSearchServiceClient = AzureSearchContext.Instance.SearchIndexClient;
            foreach (var entity in e.PublishedEntities)
            {
                azureSearchServiceClient.ReIndexContent(entity);
            }
        }
    }
}