using AutoMapper;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Umbraco.Core;

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
        }
    }
}
