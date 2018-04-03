using System.IO;
using System.Reflection;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Moriyama.AzureSearch.Tests.Unit.Config
{
    [TestFixture]
    public class ConfigurationTests
    {

        [Test]
        public void TestLoadConfig()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string config = Path.Combine(path, "AzureSearch.config.json");

            string contents = File.ReadAllText(config);

            AzureSearchConfig conf = JsonConvert.DeserializeObject<AzureSearchConfig>(contents);

            

        }

    }
}
