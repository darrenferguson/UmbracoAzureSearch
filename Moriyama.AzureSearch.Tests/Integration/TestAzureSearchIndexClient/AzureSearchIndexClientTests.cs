using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Search.Models;
using Moq;
using Moriyama.AzureSearch.Tests.Helper;
using Moriyama.AzureSearch.Umbraco.Application;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using File = System.IO.File;

namespace Moriyama.AzureSearch.Tests.Integration.TestAzureSearchIndexClient
{
    [TestFixture]
    class AzureSearchIndexClientTests
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
        public void TestDropCreateIndex()
        {

            Mock<IUmbracoDependencyHelper> umbracoDependencyHelper = new Mock<IUmbracoDependencyHelper>();
            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
                Path.GetTempPath(), umbracoDependencyHelper.Object);

            bool result = azureSearchIndexClient.DropCreateIndex();

            Assert.IsTrue(result);
        }

        [Test]
        public void TestListIndexes()
        {
            Mock<IUmbracoDependencyHelper> umbracoDependencyHelper = new Mock<IUmbracoDependencyHelper>();

            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
                Path.GetTempPath(), umbracoDependencyHelper.Object);
            Index[] indexes = azureSearchIndexClient.GetSearchIndexes();

            Assert.IsNotNull(indexes);

            foreach (Index index in indexes)
            {
                Console.WriteLine(index.Name);
            }

        }

        [Test]
        public void TestReIndexContent()
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

        }

        [Test]
        public void TestReIndexMedia()
        {
            Mock<IMediaType> mediaType = new Mock<IMediaType>();
            mediaType.Setup(x => x.Alias).Returns("umbracoContent");
            mediaType.Setup(x => x.Icon).Returns("favico.ico");

            Mock<ITemplate> template = new Mock<ITemplate>();
            template.Setup(x => x.Alias).Returns("Homepage");

            Mock<IMedia> content = new Mock<IMedia>();

            content.Setup(x => x.Id).Returns(10);
            
            content.Setup(x => x.Name).Returns("Test Media");
            content.Setup(x => x.SortOrder).Returns(1);
            content.Setup(x => x.Level).Returns(1);
            content.Setup(x => x.Path).Returns("-1,10");
            content.Setup(x => x.ParentId).Returns(-1);
            content.Setup(x => x.UpdateDate).Returns(DateTime.Now);
            content.Setup(x => x.Trashed).Returns(false);
            content.Setup(x => x.Key).Returns(Guid.NewGuid());

            content.Setup(x => x.ContentType).Returns(mediaType.Object);

            content.Setup(x => x.ContentTypeId).Returns(4);
            content.Setup(x => x.CreatorId).Returns(2);
            content.Setup(x => x.CreateDate).Returns(DateTime.Now);

            Mock<IPublishedContent> publishedContent = new Mock<IPublishedContent>();
            publishedContent.Setup(x => x.Url).Returns("/this/is/the/url");

            Mock<IUmbracoDependencyHelper> umbracoDependencyHelper = new Mock<IUmbracoDependencyHelper>();
            umbracoDependencyHelper.Setup(x => x.TypedMedia(It.IsAny<int>())).Returns(publishedContent.Object);

            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
                Path.GetTempPath(), umbracoDependencyHelper.Object);

            bool result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result);

            azureSearchIndexClient.ReIndexMedia(content.Object);
        }

        [Test]
        public void TestReIndexMember()
        {
            Mock<IMemberType> mediaType = new Mock<IMemberType>();
            mediaType.Setup(x => x.Alias).Returns("umbracoContent");
            mediaType.Setup(x => x.Icon).Returns("favico.ico");

            Mock<ITemplate> template = new Mock<ITemplate>();
            template.Setup(x => x.Alias).Returns("Homepage");

            Mock<IMember> content = new Mock<IMember>();

            content.Setup(x => x.Id).Returns(10);
            content.Setup(x => x.Email).Returns("info@moriyama.co.uk");
            content.Setup(x => x.Name).Returns("Test Member");
            content.Setup(x => x.SortOrder).Returns(1);
            content.Setup(x => x.Level).Returns(1);
            content.Setup(x => x.Path).Returns("-1,10");
            content.Setup(x => x.ParentId).Returns(-1);
            content.Setup(x => x.UpdateDate).Returns(DateTime.Now);
            content.Setup(x => x.Trashed).Returns(false);
            content.Setup(x => x.Key).Returns(Guid.NewGuid());

            content.Setup(x => x.ContentType).Returns(mediaType.Object);

            content.Setup(x => x.ContentTypeId).Returns(4);
            content.Setup(x => x.CreatorId).Returns(2);
            content.Setup(x => x.CreateDate).Returns(DateTime.Now);

          
            Mock<IUmbracoDependencyHelper> umbracoDependencyHelper = new Mock<IUmbracoDependencyHelper>();
            
            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
                Path.GetTempPath(), umbracoDependencyHelper.Object);

            bool result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result);

            azureSearchIndexClient.ReIndexMember(content.Object);
        }


        [Test]
        public void TestReIndexAll()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string wordsFile = Path.Combine(path, "words.txt");
            var lines = File.ReadAllLines(wordsFile);

            RandomText random = new RandomText(lines);


            Mock<IContentType> contentType = new Mock<IContentType>();
            contentType.Setup(x => x.Alias).Returns(random.RandomFrom(new[] { "home", "news", "content", "bulletin", "updates", "landingPage" }));
            contentType.Setup(x => x.Icon).Returns(random.RandomFrom(new[] { "tree", "home", "horse", "sheep", "cow" }) + ".ico");

            Mock<ITemplate> template = new Mock<ITemplate>();
            template.Setup(x => x.Alias).Returns(random.RandomFrom(new[] { "Home", "News", "Content", "Bulletin", "Updates", "LandingPage" }));


            IDictionary<int, IContent> contentDictionary = new Dictionary<int, IContent>();
            for (int i = 0; i <= 20; i++)
            {
               
                Mock<IContent> content = new Mock<IContent>();

                content.Setup(x => x.Id).Returns(i);

                random.AddContentParagraphs(1, 1, 1, 1, 5);
                string name = random.Content;

                content.Setup(x => x.Name).Returns(name);
                content.Setup(x => x.SortOrder).Returns(random.RandomInt(1,50));
                content.Setup(x => x.Level).Returns(random.RandomInt(1, 10));
                content.Setup(x => x.Path).Returns(random.RandomintList(10, 1, 1000));
                content.Setup(x => x.ParentId).Returns(random.RandomInt(1, 1000));
                content.Setup(x => x.UpdateDate).Returns(random.RandomDateTime());
                content.Setup(x => x.Trashed).Returns(false);
                content.Setup(x => x.Key).Returns(Guid.NewGuid());
                content.Setup(x => x.Published).Returns(true);
                content.Setup(x => x.WriterId).Returns(5);

                content.Setup(x => x.ContentTypeId).Returns(random.RandomInt(1, 30));
                content.Setup(x => x.CreatorId).Returns(random.RandomInt(1, 10));
                content.Setup(x => x.CreateDate).Returns(random.RandomDateTime());


                content.Setup(x => x.ContentType).Returns(contentType.Object);
                content.Setup(x => x.Template).Returns(template.Object);

                random.Reset();
                random.AddContentParagraphs(random.RandomInt(1, 10), 1, 10, 5, 25);
                content.Setup(x => x.HasProperty("content")).Returns(true);
                content.Setup(x => x.GetValue("content")).Returns(random.Content);

                random.Reset();
                random.AddContentParagraphs(1, 1, 1, 1, 5);
                content.Setup(x => x.HasProperty("siteTitle")).Returns(true);
                content.Setup(x => x.GetValue("siteTitle")).Returns(random.Content);

               
                contentDictionary[i] = content.Object;
            }

            Mock<IUmbracoDependencyHelper> umbracoDependencyHelper = new Mock<IUmbracoDependencyHelper>();

            // content
            umbracoDependencyHelper.Setup(x => x.DatabaseFetch<int>(It.IsRegex("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"))).Returns(random.RangeList(1,20));

            //media 
            umbracoDependencyHelper.Setup(x => x.DatabaseFetch<int>(It.IsRegex("B796F64C-1F99-4FFB-B886-4BF4BC011A9C"))).Returns(new List<int> { 4, 5, 6 });

            // members
            umbracoDependencyHelper.Setup(x => x.DatabaseFetch<int>(It.IsRegex("39EB0F98-B348-42A1-8662-E7EB18487560"))).Returns(new List<int> { 7, 8, 9 });

            
            Mock<IPublishedContent> publishedContent = new Mock<IPublishedContent>();
            publishedContent.Setup(x => x.Url).Returns(random.RandomUrl);


            umbracoDependencyHelper.Setup(x => x.TypedContent(It.IsAny<int>())).Returns(publishedContent.Object);


            Mock<IContentService> contentService = new Mock<IContentService>();
            contentService.Setup(x => x.GetByIds(It.IsAny<IEnumerable<int>>())).Returns((IEnumerable<int> ids) =>
                contentDictionary.Where(x => ids.Contains(x.Key)).Select(x => x.Value));

            umbracoDependencyHelper.Setup(x => x.GetContentService()).Returns(contentService.Object);


            IAzureSearchIndexClient azureSearchIndexClient = new AzureSearchIndexClient(this._config,
            Path.GetTempPath(), umbracoDependencyHelper.Object);

            bool result = azureSearchIndexClient.DropCreateIndex();
            Assert.IsTrue(result);

            string sid = Guid.NewGuid().ToString();
            azureSearchIndexClient.ReIndexContent(sid);

            azureSearchIndexClient.ReIndexContent(sid, 0);
        }
    }
}
