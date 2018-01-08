using Moriyama.AzureSearch.Umbraco.Application;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Moriyama.AzureSearch.Umbraco.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new AzureSearchClient(Directory.GetCurrentDirectory());

            IAzureSearchQuery query = new AzureSearchQuery("umbraco");
            var results = client.Results(query);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            query = new AzureSearchQuery().DocumentType("TextPage");
            results = client.Results(query);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            query = new AzureSearchQuery().Media();
            results = client.Results(query);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            query = new AzureSearchQuery().Media().PageSize(1);
            results = client.Results(query);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            query = new AzureSearchQuery().Content().Filter("ContentTypeId", 1056);
            results = client.Results(query);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            query = new AzureSearchQuery().Content().Contains("Path", "1070");
            results = client.Results(query);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            query = new AzureSearchQuery().Content().Contains("tags", "two");
            results = client.Results(query);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
        }
    }
}
