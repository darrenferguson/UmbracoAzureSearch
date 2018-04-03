using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Lucene.Net.Search;
using Microsoft.Azure.Search.Models;
using Moq;
using Moriyama.AzureSearch.Umbraco.Application;
using Moriyama.AzureSearch.Umbraco.Application.Examine;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models.Result;
using NUnit.Framework;
using Umbraco.Core.Models;
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;

namespace Moriyama.AzureSearch.Tests.Integration.TestExamineSearchClient
{
    [TestFixture]
    class AzureSearchExamineTests
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
                new SearchField { Name = "content", FieldType = FieldType.String, IsGridJson = true, IsSearchable = true}
            };

            AzureSearchContext azureSearchContext = AzureSearchContext.Instance;
            azureSearchContext.Initialise(this._config, Path.GetTempPath());

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

            CreateIndexResult result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result.Success);

            azureSearchIndexClient.ReIndexContent(content.Object);

          
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void TestBasicExamineSearch()
        {

          
            var examineSearcher = new DummyUmbracoExamineSearcher();


            ISearchResults results = examineSearcher.Search("hello", true);

            Assert.IsTrue(results.TotalItemCount > 0);

            foreach (var result in results)
            {
                Console.WriteLine(result.Id);
            }

        }

        [Test]
        public void TestLuceneFormatSearch()
        {
            var client = AzureSearchContext.Instance.SearchClient;

            var query = new AzureSearchQuery("content:hello").QueryType(QueryType.Full);
            var results = client.Results(query);


            Assert.IsTrue(results.Count > 0);

            foreach (var result in results.Content)
            {
                Console.WriteLine(result.Id);
            }

        }

        [Test]
        public void TestCriteriaSearch()
        {
            var client = AzureSearchContext.Instance.SearchClient;

            var searcher = new DummyUmbracoExamineSearcher();
            searcher.CreateSearchCriteria();

        }
    }
}
