using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    public class AzureSearchIndexClient : BaseAzureSearch, IAzureSearchIndexClient
    {
        // Number of docs to be processed at a time.
        const int BatchSize = 999;

        public AzureSearchIndexClient(string path) : base(path)
        {

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

            foreach (var index in indexes)
                if (index.Name == _config.IndexName)
                    serviceClient.Indexes.Delete(_config.IndexName);

            var customFields = new List<Field>();
            customFields.AddRange(GetStandardUmbracoFields());
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
            List<int> memberIds;

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

                memberIds = db.Fetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560'");
            }



            var contentCount = contentIds.Count;

            var path = Path.Combine(_path, @"App_Data\MoriyamaAzureSearch\" + sessionId);
            EnsurePath(path);

            System.IO.File.WriteAllText(Path.Combine(path, "content.json"), JsonConvert.SerializeObject(contentIds));
            System.IO.File.WriteAllText(Path.Combine(path, "media.json"), JsonConvert.SerializeObject(mediaIds));
            System.IO.File.WriteAllText(Path.Combine(path, "member.json"), JsonConvert.SerializeObject(memberIds));

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

        public AzureSearchReindexStatus ReIndexMember(string sessionId, int page)
        {
            return ReIndex("member.json", sessionId, page);
        }

        public void ReIndexContent(IContent content)
        {
            var documents = new List<Document>();
            var config = GetConfiguration();

            documents.Add(FromUmbracoContent(content, config.SearchFields));
            IndexContentBatch(documents);
        }

        public void ReIndexContent(IMedia content)
        {
            var documents = new List<Document>();
            var config = GetConfiguration();

            documents.Add(FromUmbracoMedia(content, config.SearchFields));
            IndexContentBatch(documents);
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

        public void ReIndexMember(IMember content)
        {
            var documents = new List<Document>();
            var config = GetConfiguration();

            documents.Add(FromUmbracoMember(content, config.SearchFields));
            IndexContentBatch(documents);
        }

        public AzureSearchReindexStatus ReIndex(string filename, string sessionId, int page)
        {
            var ids = GetIds(sessionId, filename);
            var conversionErrors = false;
            var failedDocumentConversionIds = new List<int>();

            var result = new AzureSearchReindexStatus
            {
                SessionId = sessionId,
                DocumentCount = ids.Length,
                Message = string.Empty
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
                {
                    if (content != null)
                    {
                        try
                        {
                            documents.Add(FromUmbracoContent(content, config.SearchFields));
                        }
                        catch (Exception ex)
                        {
                            conversionErrors = true;
                            failedDocumentConversionIds.Add(content.Id);
                        }
                    }
                }
            }
            else if (filename == "media.json")
            {
                var contents = UmbracoContext.Current.Application.Services.MediaService.GetByIds(idsToProcess);

                foreach (var content in contents)
                {
                    if (content != null)
                    {
                        try
                        {
                            documents.Add(FromUmbracoMedia(content, config.SearchFields));
                        }
                        catch (Exception ex)
                        {
                            conversionErrors = true;
                            failedDocumentConversionIds.Add(content.Id);
                        }
                    }
                }
            }
            else
            {
                var contents = new List<IMember>();

                foreach (var id in idsToProcess)
                    contents.Add(UmbracoContext.Current.Application.Services.MemberService.GetById(id));

                foreach (var content in contents)
                {
                    if (content != null)
                    {
                        try
                        {
                            documents.Add(FromUmbracoMember(content, config.SearchFields));
                        }
                        catch (Exception ex)
                        {
                            conversionErrors = true;
                            failedDocumentConversionIds.Add(content.Id);
                        }
                    }
                }
            }

            if (conversionErrors)
            {
                result.Message += $"Failed to convert Umbraco content to Azure search documents: {string.Join(", ", failedDocumentConversionIds)}\n";
            }

            var indexStatus = IndexContentBatch(documents);
            
            result.DocumentsProcessed = page * BatchSize;

            if (indexStatus.Success)
            {
                return result;
            }

            result.Error = true;
            result.Finished = true;
            result.Message += indexStatus.Message;

            return result;
        }

        private AzureSearchIndexResult IndexContentBatch(IEnumerable<Document> contents)
        {
            var serviceClient = GetClient();
            return serviceClient.IndexContentBatch(_config.IndexName, contents);
        }

        private Document FromUmbracoMember(IMember member, SearchField[] searchFields)
        {
            var result = FromUmbracoContent((ContentBase)member, searchFields);

            result.Add("IsMedia", false);
            result.Add("IsContent", false);
            result.Add("IsMember", true);

            if (member != null)
            {
                result.Add("MemberEmail", member.Email);
                result.Add("ContentTypeAlias", member.ContentType.Alias);
            }

            result.Add("Icon", member.ContentType.Icon);

            return result;
        }

        private Document FromUmbracoMedia(IMedia content, SearchField[] searchFields)
        {
            var result = FromUmbracoContent((ContentBase)content, searchFields);

            var helper = new UmbracoHelper(UmbracoContext.Current);
            var media = helper.TypedMedia(content.Id);

            if (media != null)
            {
                result.Add("Url", media.Url);
            }

            result.Add("IsMedia", true);
            result.Add("IsContent", false);
            result.Add("IsMember", false);
            result.Add("ContentTypeAlias", content.ContentType.Alias);
            result.Add("Icon", content.ContentType.Icon);

            return result;
        }

        private Document FromUmbracoContent(IContent content, SearchField[] searchFields)
        {
            var result = FromUmbracoContent((ContentBase)content, searchFields);

            result.Add("IsContent", true);
            result.Add("IsMedia", false);
            result.Add("IsMember", false);

            result.Add("Published", content.Published);
            result.Add("WriterId", content.WriterId);
            result.Add("ContentTypeAlias", content.ContentType.Alias);

            if (content.Published)
            {
                var helper = new UmbracoHelper(UmbracoContext.Current);
                var publishedContent = helper.TypedContent(content.Id);

                if (publishedContent != null)
                {
                    result.Add("Url", publishedContent.Url);
                }
            }

            // SLOW:
            //var isProtected = UmbracoContext.Current.Application.Services.PublicAccessService.IsProtected(content.Path);
            //result.Add("IsProtected", content.ContentType.Alias);

            if (content.Template != null)
                result.Add("Template", content.Template.Alias);

            result.Add("Icon", content.ContentType.Icon);

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
                {"Path", content.Path.Split(',') },
                {"ParentId", content.ParentId},
                {"UpdateDate", content.UpdateDate},
                {"Trashed", content.Trashed},
                {"Key", content.Key.ToString() }
            };

            bool cancelIndex = AzureSearch.FireContentIndexing(
                new AzureSearchEventArgs()
                {
                    Item = content,
                    Entry = c
                });

            if (cancelIndex)
            {
                // cancel was set in an event, so we don't index this item. 
                return null;
            }

            c.Add("ContentTypeId", content.ContentTypeId);
            c.Add("CreateDate", content.CreateDate);
            c.Add("CreatorId", content.CreatorId);

            foreach (var field in searchFields)
            {
                if (!content.HasProperty(field.Name) || content.Properties[field.Name].Value == null)
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
                    var value = content.Properties[field.Name].Value;

                    if (field.Type == "collection")
                    {
                        if (!string.IsNullOrEmpty(value.ToString()))
                            c.Add(field.Name, value.ToString().Split(','));
                    }
                    else
                    {
                        if (field.IsGridJson)
                        {
                            // #filth #sorrymarc
                            JObject jObject = JObject.Parse(value.ToString());
                            var tokens = jObject.SelectTokens("..value");

                            try
                            {
                                var values = tokens.Where(x => x != null).Select(x => (x as JValue).Value);
                                value = string.Join(" ", values);
                                value = Regex.Replace(value.ToString(), "<.*?>", String.Empty);
                                value = value.ToString().Replace(Environment.NewLine, " ");
                                value = value.ToString().Replace(@"\n", " ");
                            }
                            catch (Exception ex)
                            {
                                value = string.Empty;
                            }
                        }

                        c.Add(field.Name, value);
                    }
                }
            }

            AzureSearch.FireContentIndexed(
                new AzureSearchEventArgs()
                {
                    Item = content,
                    Entry = c
                });

            return c;
        }

        public Field[] GetStandardUmbracoFields()
        {
            // Key field has to be a string....
            return new[]
            {
                 new Field("Id", DataType.String) { IsKey = true, IsFilterable = true, IsSortable = true },

                 new Field("Name", DataType.String) { IsFilterable = true, IsSortable = true, IsSearchable = true, IsRetrievable = true},
                 new Field("Key", DataType.String) { IsSearchable = true, IsRetrievable = true},

                 new Field("Url", DataType.String) { IsSearchable = true, IsRetrievable = true},
                 new Field("MemberEmail", DataType.String) { IsSearchable = true },

                 new Field("IsContent", DataType.Boolean) { IsFilterable = true, IsFacetable = true },
                 new Field("IsMedia", DataType.Boolean) { IsFilterable = true, IsFacetable = true },
                 new Field("IsMember", DataType.Boolean) { IsFilterable = true, IsFacetable = true },

                 new Field("Published", DataType.Boolean) { IsFilterable = true, IsFacetable = true },
                 new Field("Trashed", DataType.Boolean) { IsFilterable = true, IsFacetable = true },

                 new Field("Path", DataType.Collection(DataType.String)) { IsSearchable = true, IsFilterable = true },
                 new Field("Template", DataType.String) { IsSearchable = true, IsFacetable = true },
                 new Field("Icon", DataType.String) { IsSearchable = true, IsFacetable = true },

                 new Field("ContentTypeAlias", DataType.String) { IsSearchable = true, IsFacetable = true, IsFilterable = true },

                 new Field("UpdateDate", DataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
                 new Field("CreateDate", DataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },

                 new Field("ContentTypeId", DataType.Int32) { IsFilterable = true },
                 new Field("ParentId", DataType.Int32) { IsFilterable = true },
                 new Field("Level", DataType.Int32) { IsSortable = true, IsFacetable = true },
                 new Field("SortOrder", DataType.Int32) { IsSortable = true },

                 new Field("WriterId", DataType.Int32) { IsSortable = true, IsFacetable = true },
                 new Field("CreatorId", DataType.Int32) { IsSortable = true, IsFacetable = true }
            };
        }
    }
}
