
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Moriyama.AzureSearch.Umbraco.Application.Extensions;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchSimpleDatasetIndexClient : BaseAzureSearch, IAzureSearchExternalIndexClient
    {
        private Dictionary<string, IComputedFieldParser> Parsers { get; set; }

        private readonly IAzureSearchSimpleDataService DataService;

        // Number of docs to be processed at a time.
        const int BatchSize = 999;

        public AzureSearchSimpleDatasetIndexClient(string path, string configPath) : base(path, configPath)
        {
            Parsers = new Dictionary<string, IComputedFieldParser>();
            var config = InitConfig<AzureSearchExternalConfig>(path, configPath);

            var simpleDatasetType = Type.GetType(config.DataService);
            if(simpleDatasetType != null)
            {
                var dataService = Activator.CreateInstance(simpleDatasetType);
                if (dataService is IAzureSearchSimpleDataService)
                    DataService = dataService as IAzureSearchSimpleDataService;
                else
                    throw new Exception(string.Format("{0} data server not of type IAzureSearchSimpleDataService", config.DataService));
            }
            else
                throw new Exception(string.Format("{0} string could not be loaded", config.DataService));
        }

        private string SessionFile(string sessionId)
        {
            var path = Path.Combine(_path, @"App_Data\MoriyamaAzureSearch");
            return Path.Combine(path, sessionId + ".json");
        }

        public override string DropCreateIndex()
        {
            var serviceClient = GetClient();
            var indexes = serviceClient.Indexes.List().Indexes;

            foreach (var index in indexes)
                if (index.Name == _config.IndexName)
                    serviceClient.Indexes.Delete(_config.IndexName);

            var customFields = new List<Field>();
            customFields.AddRange(_config.SearchFields.Select(x => x.ToAzureField()));

            var definition = new Index
            {
                Name = _config.IndexName,
                Fields = customFields
            };

            try
            {
                serviceClient.Indexes.Create(definition);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Index created";
        }

        private void EnsurePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public override AzureSearchReindexStatus ReIndexSetup(string sessionId)
        {
            List<int> ids = DataService.GetAllIds();

            var count = ids.Count;

            var path = Path.Combine(_path, @"App_Data\MoriyamaAzureSearch\" + sessionId);
            EnsurePath(path);

            System.IO.File.WriteAllText(Path.Combine(path, string.Format("{0}.json", _config.IndexName)), JsonConvert.SerializeObject(ids));

            return new AzureSearchReindexStatus
            {
                SessionId = sessionId,
                DocumentCount = count,
                Error = false,
                Finished = false
            };

        }

        private int[] GetIds(string sessionId, string filename)
        {
            var path = Path.Combine(_path, @"App_Data\MoriyamaAzureSearch\" + sessionId);
            var file = Path.Combine(path, filename);

            var ids = JsonConvert.DeserializeObject<int[]>(System.IO.File.ReadAllText(file));
            return ids;
        }

        private int[] Page(int[] collection, int page)
        {
            return collection.Skip((page - 1) * BatchSize).Take(BatchSize).ToArray();
        }

        public void Delete(int id)
        {
            var result = new AzureSearchIndexResult();

            var serviceClient = GetClient();

            var actions = new List<IndexAction>();
            var d = new Document();
            d.Add("Id", id.ToString());

            actions.Add(IndexAction.Delete(d));

            var batch = IndexBatch.New(actions);
            var indexClient = serviceClient.Indexes.GetClient(_config.IndexName);

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


            }

            result.Success = true;
        }

        public AzureSearchReindexStatus ReIndex(string sessionId, int page)
        {
            var ids = GetIds(sessionId, _config.IndexName);

            var result = new AzureSearchReindexStatus
            {
                SessionId = sessionId,
                DocumentCount = ids.Length
            };

            var idsToProcess = Page(ids, page);

            if (!idsToProcess.Any())
            {
                result.DocumentsProcessed = ids.Length;
                result.Finished = true;
                return result;
            }

            var documents = new List<Document>();
            var config = GetConfiguration();

            var allData = DataService.GettBatchData(idsToProcess);
            foreach (var data in allData)
                if (data != null)
                    documents.Add(FromData(data, config.SearchFields));

            var indexStatus = IndexContentBatch(documents);

            result.DocumentsProcessed = page * BatchSize;

            if (indexStatus.Success)
            {
                return result;
            }

            result.Error = true;
            result.Finished = true;
            result.Message = indexStatus.Message;

            return result;
        }

        private Document FromData(IAzureSearchSimpleDataSet data, SearchField[] searchFields)
        {
            var c = new Document
            {
                {"Id", data.Id.ToString()},
            };

            c = FromExternalData(c, data, searchFields);

            return c;
        }

        private Document FromExternalData(Document c, IAzureSearchSimpleDataSet data, SearchField[] searchFields)
        {

            foreach (var field in searchFields)
            {
                if (!data.RowData.ContainsKey(field.Name) || data.RowData[field.Name] == null)
                {
                    if (field.Type == "collection")
                        c.Add(field.Name, new List<string>());

                    if (field.Type == "string")
                        c.Add(field.Name, string.Empty);

                    if (field.Type == "int")
                        c.Add(field.Name, 0);

                    if (field.Type == "bool")
                        c.Add(field.Name, false);

                }
                else
                {
                    var value = data.RowData[field.Name];

                    if (field.Type == "collection")
                        c.Add(field.Name, value.Collection);

                    if (field.Type == "string")
                        c.Add(field.Name, value.String);

                    if (field.Type == "int")
                        c.Add(field.Name, value.Int);

                    if (field.Type == "bool")
                        c.Add(field.Name, value.Bool);

                    if (field.Type == "date")
                        c.Add(field.Name, value.DateTime);
                }
            }

            return c;
        }

        private AzureSearchIndexResult IndexContentBatch(IEnumerable<Document> contents)
        {
            var serviceClient = GetClient();
            return serviceClient.IndexContentBatch(_config.IndexName, contents);
        }


    }
}
