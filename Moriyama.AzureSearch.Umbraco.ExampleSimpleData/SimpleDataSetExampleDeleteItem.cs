using Umbraco.Core.Models;
using Moriyama.AzureSearch.Umbraco.Application;

namespace Moriyama.AzureSearch.Umbraco.ExampleSimpleData
{
    public class SimpleDataSetExampleDeleteItem
    {

        public void Execute(IContent subject)
        {
            var azureIndex = AzureSearchContext.Instance.SearchIndexClientCollection["simpledata"];

            if (azureIndex != null)
            {
                azureIndex.Delete(subject.Id.ToString());
            }

        }

    }
}
