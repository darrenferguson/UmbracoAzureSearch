using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Search.Models;
using Moq;
using Moriyama.AzureSearch.Umbraco.Application;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models.Result;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Tests.Integration.TestAzureSearchClient
{
    [TestFixture]
    public class AzureSearchClientTests
    {
        private AzureSearchConfig _config;
        private IUmbracoDependencyHelper _umbracoDependencyHelper;

        private void SeedContent(IAzureSearchIndexClient azureSearchIndexClient, int number)
        {
            
        }

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
                new SearchField { Name = "siteTitle", FieldType = FieldType.String, IsFilterable = true, IsSearchable = true},
                new SearchField { Name = "siteDescription", FieldType = FieldType.String, IsFilterable = true, IsSearchable = true},
                new SearchField { Name = "tags", FieldType = FieldType.Collection, IsFacetable = true},
                new SearchField { Name = "content", FieldType = FieldType.String, IsGridJson = true, IsFilterable = true, IsSearchable = true }
            };


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
            content.Setup(x => x.GetValue("siteTitle")).Returns("Integration Test");


            Mock<IPublishedContent> publishedContent = new Mock<IPublishedContent>();
            publishedContent.Setup(x => x.Url).Returns("/this/is/the/url");


            Mock<IUmbracoDependencyHelper> umbracoDependencyHelper = new Mock<IUmbracoDependencyHelper>();
            umbracoDependencyHelper.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(publishedContent.Object);

            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
                Path.GetTempPath(), umbracoDependencyHelper.Object);

            CreateIndexResult result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result.Success);

            azureSearchIndexClient.ReIndexContent(content.Object);

            // Not sure what better to do here.... waiting for the index....

            Thread.Sleep(TimeSpan.FromSeconds(5));
            this._umbracoDependencyHelper = umbracoDependencyHelper.Object;
        }

        [Test]
        public void TestSimpleSearch()
        {
            string scoringProfileJson = @"";
            IAzureSearchClient azureSearchClient = new AzureSearchClient(this._config);      
            IAzureSearchQuery query = azureSearchClient.CreateQuery("test");
        
            ISearchResult searchResult  = azureSearchClient.Results(query);

            Assert.IsTrue(searchResult.Count > 0);
            Assert.IsNotNull(searchResult.Content.FirstOrDefault(x => x.Id == 10));

            query = new AzureSearchQuery("wolf");
            searchResult = azureSearchClient.Results(query);

            Assert.IsTrue(searchResult.Count == 0);
        }


        [Test]
        public void TestHighlight()
        {
            IAzureSearchClient azureSearchClient = new AzureSearchClient(this._config);
            IAzureSearchQuery query = new AzureSearchQuery("test").Highlight("b", new [] {"Name", "siteTitle"});

            ISearchResult searchResult = azureSearchClient.Results(query);

            Assert.IsTrue(searchResult.Count > 0);
            Assert.IsNotNull(searchResult.Content.FirstOrDefault(x => x.Id == 10));

        }

        [Test]
        public void TestSuggestion()
        {

            string suggestJson = @"[
                {
                  ""name"": ""nameSuggest"",
                  ""searchMode"": ""analyzingInfixMatching"",
                  ""sourceFields"": [ ""Name"" ]
                }
              ]";

            Suggester[] suggesters = JsonConvert.DeserializeObject<Suggester[]>(suggestJson);
            this._config.Suggesters = suggesters;

            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
                Path.GetTempPath(), this._umbracoDependencyHelper);

            CreateIndexResult result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result.Success);

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
            content.Setup(x => x.GetValue("content")).Returns("Hello world Apple");

            content.Setup(x => x.HasProperty("siteTitle")).Returns(true);
            content.Setup(x => x.GetValue("siteTitle")).Returns("Integration Test");


            Mock<IPublishedContent> publishedContent = new Mock<IPublishedContent>();
            publishedContent.Setup(x => x.Url).Returns("/this/is/the/url");

            azureSearchIndexClient.ReIndexContent(content.Object);

            IAzureSearchClient azureSearchClient = new AzureSearchClient(this._config);

            // Simulate a type ahead on the word test...
            var searchResult = azureSearchClient.Suggest("tes", "nameSuggest", 5, true);

            Assert.IsTrue(searchResult.Count > 0);

        }

        [Test]
        public void TestScoringProfile()
        {
            string scoringProfileJson = @" [  
    {  
      ""name"": ""boostTitle"",  
      ""text"": {  
        ""weights"": {  
          ""siteDescription"": 1.5,  
          ""Name"": 5,  
          ""content"": 2  
        }  
      }  
    }]";

            ScoringProfile[] scoringProfile = JsonConvert.DeserializeObject<ScoringProfile[]>(scoringProfileJson);
            this._config.ScoringProfiles = scoringProfile;

            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
                Path.GetTempPath(), this._umbracoDependencyHelper);

            CreateIndexResult result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result.Success);

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
            content.Setup(x => x.GetValue("content")).Returns("Hello world Apple");

            content.Setup(x => x.HasProperty("siteTitle")).Returns(true);
            content.Setup(x => x.GetValue("siteTitle")).Returns("Integration Test");


            Mock<IPublishedContent> publishedContent = new Mock<IPublishedContent>();
            publishedContent.Setup(x => x.Url).Returns("/this/is/the/url");

            azureSearchIndexClient.ReIndexContent(content.Object);


            content = new Mock<IContent>();

            content.Setup(x => x.Id).Returns(11);
            content.Setup(x => x.Name).Returns("Test Content 2 Apple");
            content.Setup(x => x.SortOrder).Returns(2);
            content.Setup(x => x.Level).Returns(2);
            content.Setup(x => x.Path).Returns("-1,11");
            content.Setup(x => x.ParentId).Returns(-1);
            content.Setup(x => x.UpdateDate).Returns(DateTime.Now);
            content.Setup(x => x.Trashed).Returns(false);
            content.Setup(x => x.Key).Returns(Guid.NewGuid());
            content.Setup(x => x.Published).Returns(true);
            content.Setup(x => x.WriterId).Returns(6);

            content.Setup(x => x.ContentTypeId).Returns(7);
            content.Setup(x => x.CreatorId).Returns(3);
            content.Setup(x => x.CreateDate).Returns(DateTime.Now);


            content.Setup(x => x.ContentType).Returns(contentType.Object);
            content.Setup(x => x.Template).Returns(template.Object);

            content.Setup(x => x.HasProperty("content")).Returns(true);
            content.Setup(x => x.GetValue("content")).Returns("Hello wolf");

            content.Setup(x => x.HasProperty("siteTitle")).Returns(true);
            content.Setup(x => x.GetValue("siteTitle")).Returns("Integration Tests");

            azureSearchIndexClient.ReIndexContent(content.Object);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            IAzureSearchClient azureSearchClient = new AzureSearchClient(this._config);
            IAzureSearchQuery query = new AzureSearchQuery("apple").UseScoringProfile("boostTitle");

            ISearchResult searchResult = azureSearchClient.Results(query);

            Assert.IsTrue(searchResult.Count > 0);

            ISearchContent[] resultContent = searchResult.Content.ToArray();

            // The result with Apple in the title should be boosted.
            Assert.IsTrue(resultContent[0].Name.Contains("Apple"));
            Assert.IsTrue(resultContent[1].Properties["content"].ToString().Contains("Apple"));


        }

    }
}
