using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Moq;
using Moriyama.AzureSearch.Umbraco.Application;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Tests.Integration.TestAzureSearchClient
{
    [TestFixture]
    public class AzureSearchClientTests
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
        public void TestSimpleSearch()
        {

            Mock<IContentType> contentType = new Mock<IContentType>();
            contentType.Setup(x => x.Alias).Returns("umbracoContent");
            contentType.Setup(x => x.Icon).Returns("favico.ico");

            Mock<ITemplate> template = new Mock<ITemplate>();
            template.Setup(x => x.Alias).Returns("Homepage");

            Mock<IContent> content = new Mock<IContent>();

            content.Setup(x => x.Id).Returns(10);
            content.Setup(x => x.Name).Returns("Test Content");
            content.Setup(x => x.SortOrder).Returns(1);
            content.Setup(x => x.Level).Returns(1);
            content.Setup(x => x.Path).Returns("-1,10");
            content.Setup(x => x.ParentId).Returns(-1);
            content.Setup(x => x.UpdateDate).Returns(DateTime.Now);
            content.Setup(x => x.Trashed).Returns(false);
            content.Setup(x => x.Key).Returns(Guid.NewGuid());
            content.Setup(x => x.Published).Returns(true);
            content.Setup(x => x.WriterId).Returns(5);

            content.Setup(x => x.ContentTypeId).Returns(4);
            content.Setup(x => x.CreatorId).Returns(2);
            content.Setup(x => x.CreateDate).Returns(DateTime.Now);


            content.Setup(x => x.ContentType).Returns(contentType.Object);
            content.Setup(x => x.Template).Returns(template.Object);

            content.Setup(x => x.HasProperty("content")).Returns(true);
            content.Setup(x => x.GetValue("content")).Returns("Hello world");

            content.Setup(x => x.HasProperty("siteTitle")).Returns(true);
            content.Setup(x => x.GetValue("siteTitle")).Returns("Integration Tests");


            Mock<IPublishedContent> publishedContent = new Mock<IPublishedContent>();
            publishedContent.Setup(x => x.Url).Returns("/this/is/the/url");


            Mock<IUmbracoDependencyHelper> umbracoDependencyHelper = new Mock<IUmbracoDependencyHelper>();
            umbracoDependencyHelper.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(publishedContent.Object);

            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config, 
                Path.GetTempPath(), umbracoDependencyHelper.Object);

            bool result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result);

            azureSearchIndexClient.ReIndexContent(content.Object);

            IAzureSearchClient azureSearchClient = new AzureSearchClient(this._config);

            //IList<SuggestResult> results = azureSearchClient.Suggest("hello", 1, true);

            AzureSearchQuery query = new AzureSearchQuery("hello");
            ISearchResult searchResult  = azureSearchClient.Results(query);

            Assert.IsTrue(searchResult.Count > 0);
            Assert.IsNotNull(searchResult.Content.FirstOrDefault(x => x.Id == 10));

            query = new AzureSearchQuery("wolf");
            searchResult = azureSearchClient.Results(query);

            Assert.IsTrue(searchResult.Count == 0);
        }

    }
}
