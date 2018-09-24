using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Moriyama.AzureSearch.Umbraco.Application.ConstantNames;

namespace Moriyama.AzureSearch.Umbraco.Application.Extensions
{
    public static class SearchServiceClientExtensions
    {
        public static AzureSearchIndexResult IndexContentBatch(this ISearchServiceClient serviceClient, string indexName, IEnumerable<Document> contents)
        {
            var result = new AzureSearchIndexResult();
            var actions = new List<IndexAction>();

            foreach (var content in contents)
            {
                content.TryGetValue(FieldNameConstants.DoNotIndex, out var doNotIndexObject);

                bool.TryParse(doNotIndexObject?.ToString(), out var doNotIndex);

                if (doNotIndex)
                {
                    actions.Add(IndexAction.Delete(content));
                }
                else
                {
                    actions.Add(IndexAction.Upload(content));
                }                
            }

            var batch = IndexBatch.New(actions);
            var indexClient = serviceClient.Indexes.GetClient(indexName);

            try
            {
                indexClient.Documents.Index(batch);
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                var error =
                     "Failed to index some of the documents: {0}" + String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key));

                result.Success = false;
                result.Message = error;

                return result;
            }

            result.Success = true;
            return result;
        }
    }
}
