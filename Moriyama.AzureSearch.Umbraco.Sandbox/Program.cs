using Moriyama.AzureSearch.Umbraco.Application;
using System;
using System.IO;

namespace Moriyama.AzureSearch.Umbraco.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {

            var client = new AzureSearchClient(Directory.GetCurrentDirectory());

            var results = client.Search("umbraco");

            foreach(var result in results)
            {
                Console.WriteLine(result.Name);
            }
            
            // TODO:
            // All Documents by type
            // Sort
            // 
            // IsProtected - Index
            // Search by Path
            // Paging
        }
    }
}
