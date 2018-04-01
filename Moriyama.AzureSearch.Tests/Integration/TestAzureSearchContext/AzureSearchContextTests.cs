using System.Configuration;
using System.IO;
using System.Reflection;
using Moriyama.AzureSearch.Umbraco.Application;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using NUnit.Framework;

namespace Moriyama.AzureSearch.Tests.Integration.TestAzureSearchContext
{
    [TestFixture]
    public class AzureSearchContextTests
    {
        private AzureSearchConfig _config;

        [SetUp]
        public void Init()
        {
            this._config = new AzureSearchConfig()
            {
                SearchServiceName = ConfigurationManager.AppSettings["searchServiceName"],
                SearchServiceAdminApiKey = ConfigurationManager.AppSettings["searchServiceAdminApiKey"],
                IndexName = ConfigurationManager.AppSettings["indexName"],
                IndexBatchSize = 50
            };

            this._config.Fields = new SearchField[]
            {
                new SearchField { Name = "umbracoNaviHide", FieldType = FieldType.Int, IsSearchable = false, IsFilterable = true},
                new SearchField { Name = "siteTitle", FieldType = FieldType.String, IsFilterable = true},
                new SearchField { Name = "siteDescription", FieldType = FieldType.String, IsFilterable = true},
                new SearchField { Name = "tags", FieldType = FieldType.Collection, IsFacetable = true},
                new SearchField { Name = "content", FieldType = FieldType.String, IsGridJson = true}
            };
        }

        [Test]
        public void TestAzureSearchContextInit()
        {
          
            AzureSearchContext azureSearchContext = AzureSearchContext.Instance;
            azureSearchContext.Initialise(this._config, Path.GetTempPath());

            Assert.IsNotNull(azureSearchContext.SearchClient);
            Assert.IsNotNull(azureSearchContext.SearchIndexClient);   
        }
    }
}
