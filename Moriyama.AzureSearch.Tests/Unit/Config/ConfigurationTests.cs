using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Moriyama.AzureSearch.Umbraco.Application.Configuration;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Moriyama.AzureSearch.Tests.Unit.Config
{
    [TestFixture]
    public class ConfigurationTests
    {

        [Test]
        public void TestLoadConfig()
        {
            AzureSearchConfigurationSection section = (AzureSearchConfigurationSection) ConfigurationManager.GetSection("azureSearch");

            Assert.IsNotNull(section);

            Console.WriteLine(section.SearchServiceName);
            Console.WriteLine(section.SearchServiceAdminApiKey);
            Console.WriteLine(section.IndexName);

            Assert.IsNotNull(section.SearchServiceName);

            Assert.IsTrue(section.Fields.Count > 0);

            foreach (SearchFieldConfiguration field in section.Fields)
            {
                Console.WriteLine(field.Name);
            }

            Mapper.CreateMap<AzureSearchConfigurationSection, AzureSearchConfig>();
            Mapper.CreateMap<SearchFieldConfiguration, SearchField>();

            AzureSearchConfig config = Mapper.Map<AzureSearchConfig>(section);

        }

    }
}
