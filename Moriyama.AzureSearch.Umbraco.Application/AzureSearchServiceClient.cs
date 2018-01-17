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
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Moriyama.AzureSearch.Umbraco.Application.Extensions;
using Umbraco.Web.Models;
using File = System.IO.File;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchIndexClient : BaseAzureSearch, IAzureSearchIndexClient
    {
        private Dictionary<string, IComputedFieldParser> Parsers { get; set; }

        // Number of docs to be processed at a time.
        const int BatchSize = 999;

        public AzureSearchIndexClient(string path) : base(path)
        {
            Parsers = new Dictionary<string, IComputedFieldParser>();
            SetCustomFieldParsers(GetConfiguration());
        }

        private string SessionFile(string sessionId, string filename)
        {
            var path = Path.Combine(_path, @"App_Data\MoriyamaAzureSearch");
            return Path.Combine(path, sessionId, filename);
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

        private void WriteFile(string path, IEnumerable<int> ids)
        {
            var file = new FileInfo(path);
            file.Directory.Create();

            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(ids));
        }

        private void DeleteFile(string sessionId, string path)
        {
            var file = new FileInfo(SessionFile(sessionId, path));
            file.Delete();
        }

        [Obsolete]
        public AzureSearchReindexStatus ReIndexContent(string sessionId)
        {
            return new AzureSearchReindexStatus
            {
                SessionId = sessionId
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

        private List<int> FetchIds(string type)
        {
            using (var db = new UmbracoDatabase("umbracoDbDSN"))
            {
                return db.Fetch<int>($@"select distinct cmsContent.NodeId
                        from cmsContent, umbracoNode where
                        cmsContent.nodeId = umbracoNode.id and
                        umbracoNode.nodeObjectType = '{type}'");
            }
        }

        public AzureSearchReindexStatus ReIndexContent(string sessionId, int page)
        {
            var file = SessionFile(sessionId, "content.json");
            if (!File.Exists(file))
            {
                var contentIds = FetchIds(global::Umbraco.Core.Constants.ObjectTypes.Document);
                WriteFile(file, contentIds);
            }

            return ReIndex("content.json", sessionId, page);
        }

        public AzureSearchReindexStatus ReIndexMedia(string sessionId, int page)
        {
            var file = SessionFile(sessionId, "media.json");
            if (!File.Exists(file))
            {
                List<int> mediaIds;
                using (var db = new UmbracoDatabase("umbracoDbDSN"))
                {
                    mediaIds = db.Fetch<int>(@"select distinct cmsContent.NodeId
                        from cmsContent, umbracoNode where
                        cmsContent.nodeId = umbracoNode.id and
                        umbracoNode.nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C'");
                }

                WriteFile(file, mediaIds);
            }

            return ReIndex("media.json", sessionId, page);
        }

        public AzureSearchReindexStatus ReIndexMember(string sessionId, int page)
        {
            var file = SessionFile(sessionId, "member.json");
            if (!File.Exists(file))
            {
                List<int> memberIds;
                using (var db = new UmbracoDatabase("umbracoDbDSN"))
                {
                    memberIds = db.Fetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560'");
                }

                WriteFile(file, memberIds);
            }

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

        private int GetQueuedItemCount(int[] ids, int page)
        {
            var queued = ids.Length;
            if (page == 0) return queued;

            queued = ids.Length - (BatchSize * page);

            if (queued < 0)
            {
                queued = 0;
            }

            return queued;
        }

        public AzureSearchReindexStatus ReIndex(string filename, string sessionId, int page)
        {
            var ids = GetIds(sessionId, filename);

            var result = new AzureSearchReindexStatus
            {
                SessionId = sessionId
            };

            if (filename == "content.json")
            {
                result.DocumentsQueued = GetQueuedItemCount(ids, page);
            }
            else if (filename == "media.json")
            {
                result.MediaQueued = GetQueuedItemCount(ids, page);
            }
            else if (filename == "members.json")
            {
                result.MembersQueued = GetQueuedItemCount(ids, page);
            }

            if (page > 0)
            {
                var idsToProcess = Page(ids, page);

                if (!idsToProcess.Any())
                {
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
                else if (filename == "media.json")
                {
                    var contents = UmbracoContext.Current.Application.Services.MediaService.GetByIds(idsToProcess);

                    foreach (var content in contents)
                        if (content != null)
                            documents.Add(FromUmbracoMedia(content, config.SearchFields));
                }
                else if (filename == "members.json")
                {
                    var contents = new List<IMember>();

                    foreach (var id in idsToProcess)
                        contents.Add(UmbracoContext.Current.Application.Services.MemberService.GetById(id));

                    foreach (var content in contents)
                        if (content != null)
                            documents.Add(FromUmbracoMember(content, config.SearchFields));
                }

                var indexStatus = IndexContentBatch(documents);

                if (!indexStatus.Success)
                    result.Error = true;

                var totalPages = (int) Math.Ceiling((double) (ids.Length / BatchSize)) + 1;
                if (page == totalPages)
                {
                    DeleteFile(sessionId, filename);
                    result.Message = "Done";
                }
                else
                {
                    result.Message = $"Sent {filename.Replace(".json", "")} page {page + 1} of {totalPages} for indexing. {indexStatus.Message}";
                }
            }

            return result;
        }

        private AzureSearchIndexResult IndexContentBatch(IEnumerable<Document> contents)
        {
            var serviceClient = GetClient();
            return serviceClient.IndexContentBatch(_config.IndexName, contents);
        }

        private Document FromUmbracoMember(IMember member, SearchField[] searchFields)
        {
            var result = GetDocumentToIndex((ContentBase)member, searchFields);

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
            var result = GetDocumentToIndex((ContentBase)content, searchFields);

            if (!content.ContentType.Alias.Equals("Folder"))
            {
                if (content.HasProperty("umbracoFile"))
                {
                    var value = (content.Properties?["umbracoFile"]?.Value ?? "").ToString();

                    if (value.StartsWith("{") && value.EndsWith("}"))
                    {
                        try
                        {
                            var obj = JsonConvert.DeserializeObject<ImageCropDataSet>(value);
                            value = obj.Src;
                        }
                        catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException)
                        {
                            value = string.Empty;
                        }
                    }

                    result.Add("Url", value);
                }
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
            var result = GetDocumentToIndex((ContentBase)content, searchFields);

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

        private Document GetDocumentToIndex(IContentBase content, SearchField[] searchFields)
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

            var umbracoFields = searchFields.Where(x => !x.IsComputedField()).ToArray();
            var computedFields = searchFields.Where(x => x.IsComputedField()).ToArray();

            c = FromUmbracoContentBase(c, content, umbracoFields);
            c = FromComputedFields(c, content, computedFields);

            AzureSearch.FireContentIndexed(
                new AzureSearchEventArgs()
                {
                    Item = content,
                    Entry = c
                });

            return c;
        }

        private Document FromUmbracoContentBase(Document c, IContentBase content, SearchField[] searchFields)
        {
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

            return c;
        }

        private Document FromComputedFields(Document document, IContentBase content, SearchField[] customFields)
        {
            if (customFields != null)
            {
                foreach (var customField in customFields)
                {
                    var parser = Parsers.Single(x => x.Key == customField.ParserType).Value;
                    document.Add(customField.Name, parser.GetValue(content));
                }
            }

            return document;
        }

        private void SetCustomFieldParsers(AzureSearchConfig azureSearchConfig)
        {
            if (azureSearchConfig.SearchFields != null)
            {
                var types = azureSearchConfig.SearchFields.Where(x => x.IsComputedField()).Select(x => x.ParserType).Distinct().ToArray();

                foreach (var t in types)
                {
                    var parser = Activator.CreateInstance(Type.GetType(t));

                    if (!(parser is IComputedFieldParser))
                    {
                        throw new Exception(string.Format("Type {0} does not implement {1}", t, typeof(IComputedFieldParser).Name));
                    }

                    Parsers.Add(t, (IComputedFieldParser)parser);
                }
            }
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
