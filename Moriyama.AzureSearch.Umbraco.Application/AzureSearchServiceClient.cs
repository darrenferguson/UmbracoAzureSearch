using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchServiceClient : IAzureSearchServiceClient
    {
        private readonly AzureSearchConfig _config;
        private readonly string _path;

        // Number of docs to be processed at a time.
        const int BatchSize = 999;

        public AzureSearchServiceClient(string path)
        {
            _path = path;
            _config = JsonConvert.DeserializeObject<AzureSearchConfig>(System.IO.File.ReadAllText(Path.Combine(path, @"config\AzureSearch.config")));
        }

        public AzureSearchConfig GetConfiguration()
        {
            return _config;
        }

        private SearchServiceClient GetClient()
        {
            var serviceClient = new SearchServiceClient(_config.SearchServiceName, new SearchCredentials(_config.SearchServiceAdminApiKey));
            return serviceClient;
        }

        private string SessionFile(string sessionId)
        {
            var path = Path.Combine(_path, @"App_Data\MoriyamaAzureSearch");
            return Path.Combine(path, sessionId + ".json");
        }

        public string DropCreateIndex()
        {
            
            var serviceClient = GetClient();
            var indexes = serviceClient.Indexes.List().Indexes;

            foreach(var index in indexes)
                if(index.Name == _config.IndexName)
                    serviceClient.Indexes.Delete(_config.IndexName);

            var customFields = new List<Field>();
            customFields.AddRange(GetStandardUmbracoFields());

            foreach(var field in _config.SearchFields)
            {
                DataType t = DataType.String;
                switch (field.Type.ToLower())
                {
                    case "bool":
                        t = DataType.Boolean;
                        break;
                    case "int":
                        t = DataType.Int64;
                        break;
                }

                var f = new Field()
                {
                    Name = field.Name,
                    Type = t,
                    IsFacetable = field.IsFacetable,
                    IsFilterable = field.IsFilterable,
                    IsSortable = field.IsSortable
                    
                };
                
                customFields.Add(f);
            }

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

        public Index[] GetSearchIndexes()
        {
            var serviceClient = GetClient();
            var indexes = serviceClient.Indexes.List().Indexes;
            return indexes.ToArray();
        }

        private void EnsurePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public AzureSearchReindexStatus ReIndexContent(string sessionId)
        {
            List<int> contentIds;
            List<int> mediaIds;

            using (var db = new UmbracoDatabase("umbracoDbDSN"))
            {
                contentIds = db.Fetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = 'C66BA18E-EAF3-4CFF-8A22-41B16D66A972'");

                mediaIds = db.Fetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C'");
            }

            var contentCount = contentIds.Count;

            var path = Path.Combine(_path, @"App_Data\MoriyamaAzureSearch\" + sessionId);
            EnsurePath(path);
                   
            System.IO.File.WriteAllText(Path.Combine(path, "content.json"), JsonConvert.SerializeObject(contentIds));
            System.IO.File.WriteAllText(Path.Combine(path, "media.json"), JsonConvert.SerializeObject(mediaIds));

            return new AzureSearchReindexStatus
            {
                SessionId = sessionId,
                DocumentCount = contentCount,
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
      
        public AzureSearchReindexStatus ReIndexContent(string sessionId, int page)
        {
            return ReIndex("content.json", sessionId, page);
        }

        public AzureSearchReindexStatus ReIndexMedia(string sessionId, int page)
        {
            return ReIndex("media.json", sessionId, page);
        }

        public AzureSearchReindexStatus ReIndex(string filename, string sessionId, int page)
        {
            var ids = GetIds(sessionId, filename);

            var result = new AzureSearchReindexStatus {
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

            if (filename == "content.json")
            {
                var contents = UmbracoContext.Current.Application.Services.ContentService.GetByIds(idsToProcess);
                foreach (var content in contents)
                    if (content != null)
                        documents.Add(FromUmbracoContent(content, config.SearchFields));

            }
            else
            {
                var contents = UmbracoContext.Current.Application.Services.MediaService.GetByIds(idsToProcess);

                foreach (var content in contents)
                    if (content != null)
                        documents.Add(FromUmbracoMedia(content, config.SearchFields));
            }

            var indexStatus = IndexContentBatch(documents);

            result.DocumentsProcessed = page * BatchSize;

            if(indexStatus.Success)
            {
                return result;
            }

            result.Error = true;
            result.Finished = true;
            result.Message = indexStatus.Message;

            return result;                 
        }

        private AzureSearchIndexResult IndexContentBatch(IEnumerable<Document> contents)
        {
            var result = new AzureSearchIndexResult();

            var serviceClient = GetClient();
            
            var actions = new List<IndexAction>();

            foreach (var content in contents)
                actions.Add(IndexAction.Upload(content));

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
                
                return result;
            }

            result.Success = true;
            return result;
        }

        private Document FromUmbracoMedia(IMedia content, SearchField[] searchFields)
        {
            var result = FromUmbracoContent((ContentBase)content, searchFields);

            result.Add("IsMedia", true);
            result.Add("IsContent", false);
            result.Add("ContentTypeAlias", content.ContentType.Alias);
            return result;
        }

        private Document FromUmbracoContent(IContent content, SearchField[] searchFields)
        {
            var result = FromUmbracoContent((ContentBase) content, searchFields);

            result.Add("IsContent", true);
            result.Add("IsMedia", false);

            result.Add("Published", content.Published);
            result.Add("WriterId", content.WriterId);
            result.Add("ContentTypeAlias", content.ContentType.Alias);

            if (content.Template != null)
                result.Add("Template", content.Template.Alias);

            return result;
        }
        
        private Document FromUmbracoContent(IContentBase content, SearchField[] searchFields)
        {
            var c = new Document
            {
                {"Id", content.Id.ToString()},
                {"Name", content.Name},             
                {"SortOrder", content.SortOrder},
                {"Level", content.Level},
                {"Path", content.Path},
                {"ParentId", content.ParentId},
                {"UpdateDate", content.UpdateDate},
                {"Trashed", content.Trashed},
                
            };

            c.Add("ContentTypeId", content.ContentTypeId);
            c.Add("CreateDate", content.CreateDate);
            c.Add("CreatorId", content.CreatorId);
            
            foreach(var field in searchFields)
            {
                if (!content.HasProperty(field.Name))
                    continue;

                c.Add(field.Name, content.Properties[field.Name].Value);
            }

            return c;
        }

        public Field[] GetStandardUmbracoFields()
        {
            // Key field has to be a string....
            return new[]
            {
                 new Field("Id", DataType.String) { IsKey = true, IsFilterable = true, IsSortable = true },
                 new Field("Name", DataType.String) { IsSortable = true, IsSearchable = true, IsRetrievable = true},

                 new Field("IsContent", DataType.Boolean) { IsFilterable = true, IsFacetable = true },
                 new Field("IsMedia", DataType.Boolean) { IsFilterable = true, IsFacetable = true },
            
                 new Field("Published", DataType.Boolean) { IsFilterable = true, IsFacetable = true },
                 new Field("Trashed", DataType.Boolean) { IsFilterable = true, IsFacetable = true },

                 new Field("Path", DataType.String) { IsSearchable = true },
                 new Field("Template", DataType.String) { IsSearchable = true, IsFacetable = true },
                 new Field("ContentTypeAlias", DataType.String) { IsSearchable = true, IsFacetable = true },

                 new Field("UpdateDate", DataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
                 new Field("CreateDate", DataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },

                 new Field("ContentTypeId", DataType.Int64) { IsFilterable = true },
                 new Field("ParentId", DataType.Int64) { IsFilterable = true },
                 new Field("Level", DataType.Int64) { IsSortable = true, IsFacetable = true },
                 new Field("SortOrder", DataType.Int64) { IsSortable = true },

                 new Field("WriterId", DataType.Int64) { IsSortable = true, IsFacetable = true },
                 new Field("CreatorId", DataType.Int64) { IsSortable = true, IsFacetable = true }
            };
        }
    }
}
