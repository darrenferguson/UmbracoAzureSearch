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
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using log4net;
using Moriyama.AzureSearch.Umbraco.Application.Extensions;

namespace Moriyama.AzureSearch.Umbraco.Application
{
    public class AzureSearchIndexClient : BaseAzureSearch, IAzureSearchIndexClient
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IComputedFieldParser> Parsers { get; set; }

        // Number of docs to be processed at a time.
        private readonly int _batchSize;
        private readonly string _tempDirectory;
        private readonly IUmbracoDependencyHelper _umbracoDependencyHelper;
        
        public event EventHandler<Index> CreatingIndex;

        public AzureSearchIndexClient(AzureSearchConfig configuration, string tempDirectory, IUmbracoDependencyHelper umbracoDependencyHelper) : base(configuration)
        {
            this._umbracoDependencyHelper = umbracoDependencyHelper;
            this._batchSize = configuration.IndexBatchSize;
            this._tempDirectory = tempDirectory;

            Parsers = new Dictionary<string, IComputedFieldParser>();
            SetCustomFieldParsers(GetConfiguration());
        }

        public bool DropCreateIndex()
        {
            var serviceClient = GetClient();
            var indexes = serviceClient.Indexes.List().Indexes;

            foreach (var index in indexes)
            {
                if (index.Name == this._configuration.IndexName)
                {
                    serviceClient.Indexes.Delete(this._configuration.IndexName);
                }
            }

            List<Field> customFields = new List<Field>();
            customFields.AddRange(GetStandardUmbracoFields());
            customFields.AddRange(this._configuration.Fields.Select(x => x.ToAzureField()));

            Index indexDefinition = new Index
            {
                Name = this._configuration.IndexName,
                Fields = customFields,
                ScoringProfiles = this._configuration.ScoringProfiles,
                Analyzers = this._configuration.Analyzers

            };

            try
            {
                CreatingIndex?.Invoke(this, indexDefinition);
                serviceClient.Indexes.Create(indexDefinition);
            }
            catch (Exception ex)
            {
                Log.Error($"Can't create index {this._configuration.IndexName}", ex);
                return false;
            }

            return true;
        }

        public Index[] GetSearchIndexes()
        {
            ISearchServiceClient serviceClient = GetClient();
            IList<Index> indexes = serviceClient.Indexes.List().Indexes;
            return indexes.ToArray();
        }

        private void EnsurePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public AzureSearchReindexStatus ReIndexContent(string sessionId)
        {
            // TODO - store query an guids somewhere else... guids in meaninful constants.
            // Query in one place.

            IList<int> contentIds = this._umbracoDependencyHelper.DatabaseFetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = 'C66BA18E-EAF3-4CFF-8A22-41B16D66A972'");

            IList<int> mediaIds = this._umbracoDependencyHelper.DatabaseFetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C'");

            IList<int> memberIds = this._umbracoDependencyHelper.DatabaseFetch<int>(@"select distinct cmsContent.NodeId
                    from cmsContent, umbracoNode where
                    cmsContent.nodeId = umbracoNode.id and
                    umbracoNode.nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560'");
            
            
            string path = Path.Combine(this._tempDirectory, sessionId);
            EnsurePath(path);

            System.IO.File.WriteAllText(Path.Combine(path, "content.json"), JsonConvert.SerializeObject(contentIds));
            System.IO.File.WriteAllText(Path.Combine(path, "media.json"), JsonConvert.SerializeObject(mediaIds));
            System.IO.File.WriteAllText(Path.Combine(path, "member.json"), JsonConvert.SerializeObject(memberIds));

            return new AzureSearchReindexStatus
            {
                SessionId = sessionId,
                ContentCount = contentIds.Count,
                MediaCount = mediaIds.Count,
                MemberCount = mediaIds.Count,
                Error = false,
                Finished = false
            };
        }

        private int[] GetIds(string sessionId, string filename)
        {
            string file = Path.Combine(this._tempDirectory, sessionId, filename);
            return JsonConvert.DeserializeObject<int[]>(System.IO.File.ReadAllText(file));
        }

        private int[] Page(int[] collection, int page)
        {
            return collection.Skip((page - 1) * this._batchSize).Take(this._batchSize).ToArray();
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
            IList<Document> documents = new List<Document>();
            AzureSearchConfig config = GetConfiguration();

            documents.Add(FromUmbracoContent(content, config.Fields));
            IndexContentBatch(documents);
        }

        public void ReIndexMedia(IMedia content)
        {
            var documents = new List<Document>();
            var config = GetConfiguration();

            documents.Add(FromUmbracoMedia(content, config.Fields));
            IndexContentBatch(documents);
        }

        public AzureSearchIndexResult Delete(int id)
        {
            AzureSearchIndexResult result = new AzureSearchIndexResult();

            ISearchServiceClient serviceClient = GetClient();

            IList <IndexAction> actions = new List<IndexAction>();
            var document = new Document();

            document.Add("Id", id.ToString());

            actions.Add(IndexAction.Delete(document));

            IndexBatch batch = IndexBatch.New(actions);

            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(_configuration.IndexName);

            try
            {
                indexClient.Documents.Index(batch);
            }
            catch (IndexBatchException indexBatchException)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                string  error =
                     "Failed to index some of the documents: {0}" + string.Join(", ", indexBatchException.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key));

                result.Success = false;
                result.Message = error;

                Log.Warn(indexBatchException);
                return result;
            }

            result.Success = true;
            return result;
        }

        public void ReIndexMember(IMember content)
        {
            IList<Document> documents = new List<Document>();
            AzureSearchConfig config = GetConfiguration();

            documents.Add(FromUmbracoMember(content, config.Fields));
            IndexContentBatch(documents);
        }

        public AzureSearchReindexStatus ReIndex(string filename, string sessionId, int page)
        {
            int[] ids = GetIds(sessionId, filename);

            AzureSearchReindexStatus result = new AzureSearchReindexStatus
            {
                SessionId = sessionId,
                ContentCount = ids.Length
            };

            int[] idsToProcess = Page(ids, page);

            if (!idsToProcess.Any())
            {
                result.DocumentsProcessed = ids.Length;
                result.Finished = true;
                return result;
            }

            IList<Document> documents = new List<Document>();
            AzureSearchConfig config = GetConfiguration();

            if (filename == "content.json")
            {
                IEnumerable<IContent> contents = this._umbracoDependencyHelper.GetContentService().GetByIds(idsToProcess);

                foreach (IContent content in contents)
                {
                    if (content != null)
                    {
                        documents.Add(FromUmbracoContent(content, config.Fields));
                    }
                }
            }
            else if (filename == "media.json")
            {
                IEnumerable<IMedia> medias = this._umbracoDependencyHelper.GetMediaService().GetByIds(idsToProcess);

                foreach (IMedia media in medias)
                {
                    if (media != null)
                    {
                        documents.Add(FromUmbracoMedia(media, config.Fields));
                    }
                }
            }
            else
            {
                IList<IMember> members = new List<IMember>();

                foreach (int id in idsToProcess)
                {
                    members.Add(this._umbracoDependencyHelper.GetMemberService().GetById(id));
                }

                foreach (IMember content in members)
                {
                    if (content != null)
                    {
                        documents.Add(FromUmbracoMember(content, config.Fields));
                    }
                }
            }

            AzureSearchIndexResult indexStatus = IndexContentBatch(documents);

            result.DocumentsProcessed = page * this._batchSize;

            if (indexStatus.Success)
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
            ISearchServiceClient serviceClient = GetClient();
            return serviceClient.IndexContentBatch(_configuration.IndexName, contents);
        }

        private Document FromUmbracoMember(IMember member, SearchField[] searchFields)
        {
            Document result = GetDocumentToIndex(member, searchFields);

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
            Document result = GetDocumentToIndex(content, searchFields);

            
            IPublishedContent media = this._umbracoDependencyHelper.TypedMedia(content.Id);

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
            Document result = GetDocumentToIndex(content, searchFields);

            result.Add("IsContent", true);
            result.Add("IsMedia", false);
            result.Add("IsMember", false);

            result.Add("Published", content.Published);
            result.Add("WriterId", content.WriterId);
            result.Add("ContentTypeAlias", content.ContentType.Alias);

            if (content.Published)
            {
                
                IPublishedContent publishedContent = this._umbracoDependencyHelper.TypedContent(content.Id);

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
            var document = new Document
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
                    Entry = document
                });

            if (cancelIndex)
            {
                // cancel was set in an event, so we don't index this item. 
                return null;
            }

            var umbracoFields = searchFields.Where(x => !x.IsComputedField()).ToArray();
            var computedFields = searchFields.Where(x => x.IsComputedField()).ToArray();

            document = FromUmbracoContentBase(document, content, umbracoFields);
            document = FromComputedFields(document, content, computedFields);

            AzureSearch.FireContentIndexed(
                new AzureSearchEventArgs()
                {
                    Item = content,
                    Entry = document
                });

            return document;
        }

        private Document FromUmbracoContentBase(Document document, IContentBase content, SearchField[] searchFields)
        {
            document.Add("ContentTypeId", content.ContentTypeId);
            document.Add("CreateDate", content.CreateDate);
            document.Add("CreatorId", content.CreatorId);

            foreach (SearchField field in searchFields)
            {
                object value = content.GetValue(field.Name);

                if (value == null)
                {

                    if (field.FieldType == FieldType.Collection)
                        document.Add(field.Name, new List<string>());

                    if (field.FieldType == FieldType.String)
                        document.Add(field.Name, string.Empty);

                    if (field.FieldType == FieldType.Int)
                        document.Add(field.Name, 0);

                    if (field.FieldType == FieldType.Bool)
                        document.Add(field.Name, false);
                }
                else
                {
                    
                    if (field.FieldType == FieldType.Collection)
                    {
                        if (!string.IsNullOrEmpty(value.ToString()))
                            document.Add(field.Name, value.ToString().Split(','));
                    }
                    else
                    {
                        if (field.IsGridJson)
                        {
                            // #filth #sorrymarc
                            JObject jObject;
                            try
                            {
                                jObject = JObject.Parse(value.ToString());
                            }
                            catch (JsonReaderException jsonReaderException)
                            {
                                Log.Warn($"Invalid json for {field.Name}", jsonReaderException);
                                document.Add(field.Name, value);
                                continue;
                            }

                            IEnumerable<JToken> tokens = jObject.SelectTokens("..value");

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
                                Log.Warn("Can't parse Grid JSON", ex);
                                value = string.Empty;
                            }
                        }

                        document.Add(field.Name, value);
                    }
                }
            }

            return document;
        }

        private Document FromComputedFields(Document document, IContentBase content, SearchField[] customFields)
        {
            if (customFields != null)
            {
                foreach (var customField in customFields)
                {
                    IComputedFieldParser parser = Parsers.Single(x => x.Key == customField.ParserType).Value;
                    document.Add(customField.Name, parser.GetValue(content));
                }
            }

            return document;
        }

        private void SetCustomFieldParsers(AzureSearchConfig azureSearchConfig)
        {
            if (azureSearchConfig.Fields != null)
            {
                string[] types = azureSearchConfig.Fields.Where(x => x.IsComputedField()).Select(x => x.ParserType).Distinct().ToArray();

                foreach (var typeName in types)
                {
                    var parser = Activator.CreateInstance(Type.GetType(typeName));

                    if (!(parser is IComputedFieldParser))
                    {
                        throw new Exception(string.Format("Type {0} does not implement {1}", typeName, typeof(IComputedFieldParser).Name));
                    }

                    Parsers.Add(typeName, (IComputedFieldParser)parser);
                }
            }
        }

        public Field[] GetStandardUmbracoFields()
        {
            // Key field has to be a string....
            Field key = new Field("Id", DataType.String) { IsKey = true, IsFilterable = true, IsSortable = true };

            Field[] fields = new[]
            {
                    
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

            IList<Field> sorted = new List<Field>(fields.OrderBy(f => f.Name));
            sorted.Insert(0, key);

            return sorted.ToArray();

        }
    }
}
