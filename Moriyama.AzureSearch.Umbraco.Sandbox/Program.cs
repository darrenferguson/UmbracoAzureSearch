using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace Moriyama.AzureSearch.Umbraco.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {

            var client = new AzureSearchClient(Directory.GetCurrentDirectory());

			var config = client.GetConfiguration();

			int count = config.ScoringProfiles.Count;
			var scoringProfiles = config.ScoringProfiles.Select(x => x.GetEffectiveScoringProfile()).ToList();
						
            var results = client.Term("umbraco").Results();
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            client = new AzureSearchClient(Directory.GetCurrentDirectory());
            results = client.DocumentType("TextPage").Results();
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));


            client = new AzureSearchClient(Directory.GetCurrentDirectory());
            results = client.Media().Results();
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            client = new AzureSearchClient(Directory.GetCurrentDirectory());
            results = client.Media().PageSize(1).Results();
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));


            client = new AzureSearchClient(Directory.GetCurrentDirectory());
            results = client.Content().Filter("ContentTypeId", 1056).Results();

            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            client = new AzureSearchClient(Directory.GetCurrentDirectory());
            results = client.Content().Contains("Path", "1070").Results();

            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

            client = new AzureSearchClient(Directory.GetCurrentDirectory());
            results = client.Content().Contains("tags", "two").Results();

            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));


            // IsProtected - Index         
        }
    }
}
