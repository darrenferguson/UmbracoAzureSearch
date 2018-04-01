using System.Configuration;
using System.IO;
using AutoMapper;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Configuration;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Moriyama.AzureSearch.Umbraco.Application.Umbraco
{
    public class CustomApplicationEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {           
            bool.TryParse(ConfigurationManager.AppSettings["Moriyama.AzureSearch:InitialiseContextOnStartup"], out bool init);

            if (!init)
            {
                // Init of singleton context has been disabled in web.config
                return;
            }

            Mapper.CreateMap<DataType, FieldType>().ConvertUsing(value =>
            {
                if (value.Equals(DataType.Boolean))
                {
                    return FieldType.Bool;
                }

                if (value.Equals(DataType.DateTimeOffset))
                {
                    return FieldType.Date;
                }

                if (value.Equals(DataType.Int32))
                {
                    return FieldType.Int;
                }

                if (value.Equals(DataType.Int64))
                {
                    return FieldType.Int;
                }

                if (value.Equals(DataType.Int64))
                {
                    return FieldType.Int;
                }

                return FieldType.String;
            });

            Mapper.CreateMap<Field, SearchField>().ForMember(dest => dest.FieldType, opts => opts.MapFrom(src => src.Type));

            Mapper.CreateMap<Index, SearchIndex>();
            Mapper.CreateMap<AzureSearchConfigurationSection, AzureSearchConfig>();
            Mapper.CreateMap<SearchFieldConfiguration, SearchField>();
           
            AzureSearchConfigurationSection section = (AzureSearchConfigurationSection)ConfigurationManager.GetSection("azureSearch");
            AzureSearchConfig config = Mapper.Map<AzureSearchConfig>(section);

            string tempPath = Path.GetTempPath();

            // You may want to use a custom directory if hosting permissions require.
            if (!string.IsNullOrEmpty(section.TempDirectory))
            {
                if (Path.IsPathRooted(section.TempDirectory))
                {
                    tempPath = section.TempDirectory;
                }
                else
                {
                    tempPath = IOHelper.MapPath(section.TempDirectory);
                }

                if (!Directory.Exists(tempPath))
                {
                    tempPath = Path.GetTempPath();
                }
            }

            AzureSearchContext.Instance.Initialise(config, tempPath);
            
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
        
        private void ContentServiceEmptiedRecycleBin(IContentService sender, RecycleBinEventArgs eventArgs)
        {
            foreach (int id in eventArgs.Ids)
            {
                AzureSearchContext.Instance.SearchIndexClient.Delete(id);
            }
        } 

        private void MediaServiceDeleted(IMediaService sender, DeleteEventArgs<IMedia> eventArgs)
        {
            foreach (IMedia media in eventArgs.DeletedEntities)
            {
                AzureSearchContext.Instance.SearchIndexClient.Delete(media.Id);
            }
        }

        private void ContentServiceDeleted(IContentService sender, DeleteEventArgs<IContent> eventArgs)
        {
            foreach (IContent content in eventArgs.DeletedEntities)
            {
                AzureSearchContext.Instance.SearchIndexClient.Delete(content.Id);
            }
        }

        private void ContentServiceSaved(IContentService sender, SaveEventArgs<IContent> eventArgs)
        {
            foreach (IContent content in eventArgs.SavedEntities)
            {
                AzureSearchContext.Instance.SearchIndexClient.ReIndexContent(content);
            }
        }

        private void MemberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> eventArgs)
        {
            foreach (IMember member in eventArgs.DeletedEntities)
            {
                AzureSearchContext.Instance.SearchIndexClient.Delete(member.Id);
            }
        }

        private void ContentServiceTrashed(IContentService sender, MoveEventArgs<IContent> eventArgs)
        {
            foreach (MoveEventInfo<IContent> item in eventArgs.MoveInfoCollection)
            {
                AzureSearchContext.Instance.SearchIndexClient.ReIndexContent(item.Entity);
            }
        }

        private void MediaServiceTrashed(IMediaService sender, MoveEventArgs<IMedia> eventArgs)
        {
            foreach (MoveEventInfo<IMedia> item in eventArgs.MoveInfoCollection)
            {
                AzureSearchContext.Instance.SearchIndexClient.ReIndexMedia(item.Entity);
            }
        }

        private void MemberServiceSaved(IMemberService sender, SaveEventArgs<IMember> eventArgs)
        {
            foreach (IMember member in eventArgs.SavedEntities)
            {
                AzureSearchContext.Instance.SearchIndexClient.ReIndexMember(member);
            }
        }

        private void MediaServiceSaved(IMediaService sender, SaveEventArgs<IMedia> eventArgs)
        {
            foreach (IMedia media in eventArgs.SavedEntities)
            {
                AzureSearchContext.Instance.SearchIndexClient.ReIndexMedia(media);
            }   
        }

        private void ContentServicePublished(IPublishingStrategy sender, PublishEventArgs<IContent> eventArgs)
        {
            foreach (IContent content in eventArgs.PublishedEntities)
            {
                AzureSearchContext.Instance.SearchIndexClient.ReIndexContent(content);
            }
        }
    }
}