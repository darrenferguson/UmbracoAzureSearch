using Microsoft.Azure.Search.Models;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchConstants
    {
        public const string MainConfigFile = @"config\AzureSearch.config";
        public const string AdditionalConfigFilePattern = @"config\AzureSearch.*.config";

        public const string UmbracoIndexName = "umbraco";
    }
}