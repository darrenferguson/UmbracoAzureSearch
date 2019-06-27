using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Moriyama.AzureSearch.Umbraco.Application.Extensions
{
    public static class SearchServiceClientExtensions
    {
        public static AzureSearchIndexResult IndexContentBatch(this SearchServiceClient serviceClient, string indexName, IEnumerable<Document> contents)
        {
			var result = new AzureSearchIndexResult();

			if (contents == null || !contents.Any())
			{
				result.Success = true;
				result.Message = $"{nameof(IndexContentBatch)} received no content to index for {indexName}";
				return result;
			}
            
            var actions = new List<IndexAction>();
            foreach (var content in contents)
            {
                actions.Add(IndexAction.Upload(content));
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
				string failedDocuments = String.Join(",", e?.IndexingResults?.Where(r => !r.Succeeded).Select(r => r.Key));                
                result.Message = $"IndexBatchException {e.Message} indexing {failedDocuments}";
				result.Success = false;
				return result;
            }

            result.Success = true;
            return result;
        }
    }
}
