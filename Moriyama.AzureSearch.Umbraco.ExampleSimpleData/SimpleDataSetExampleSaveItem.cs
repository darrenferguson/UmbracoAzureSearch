using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application;
using Umbraco.Core.Models;

//Moriyama.AzureSearch.Umbraco.Application.AzureSearchSimpleDataSetExampleService

namespace Moriyama.AzureSearch.Umbraco.ExampleSimpleData
{
    public class SimpleDataSetExampleSaveItem
    {

        public void Execute(IContent subject)
        {
            var azureIndex = AzureSearchContext.Instance.SearchIndexClientCollection["simpledata"] as IAzureSearchSimpleDataSetIndexClient;

            if (azureIndex != null)
            {
                var simpleDataService = new AzureSearchSimpleDataSetExampleService();
                var dataset = simpleDataService.Get(subject);

                if (dataset != null)
                {
                    azureIndex.ReIndexItem(dataset);
                }
                else
                {
                    azureIndex.Delete(subject.Id.ToString());
                }
            }

        }

    }
}
