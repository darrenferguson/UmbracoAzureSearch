using System.IO;
using System.Reflection;
using Moriyama.AzureSearch.Umbraco.Application;
using NUnit.Framework;

namespace Moriyama.AzureSearch.Tests.Integration.TestAzureSearchContext
{
    [TestFixture]
    public class AzureSearchContextTests
    {
        [Test]
        public void TestAzureSearchContextInit()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = Path.Combine(path, "AzureSearch.config");

            AzureSearchContext azureSearchContext = AzureSearchContext.Instance;
            azureSearchContext.Initialise(configFile, 999, Path.GetTempPath());

            Assert.IsNotNull(azureSearchContext.SearchClient);
            Assert.IsNotNull(azureSearchContext.SearchIndexClient);   
        }
    }
}
