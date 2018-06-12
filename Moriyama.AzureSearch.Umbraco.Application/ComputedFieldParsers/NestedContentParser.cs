using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Moriyama.AzureSearch.Umbraco.Application.ComputedFieldParsers
{
    /// <summary>
    /// Parses the content of a nested content field.
    /// </summary>
    public class NestedContentParser : IComputedFieldParser
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public object GetValue(IContentBase content, string fieldName, IAzureSearchIndexClient azureSearchIndexClient)
        {
            var fields = azureSearchIndexClient.GetConfiguration().Fields;

            var fieldContents = content.GetValue<string>(fieldName);

            if (fieldContents == null)
            {
                return null;
            }

            // Nested content is always an array.
            if (!fieldContents.StartsWith("["))
            {
                return null;
            }

            try
            {
                // Convert the JSON string to a list of Dictionaries.
                var propertyDictionaries = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(fieldContents);

                var sb = new StringBuilder();

                ProcessPropertyDictionaries(sb, propertyDictionaries, fields, false);

                return sb.ToString();
            }
            catch (Exception e)
            {
                Log.Error("Error deserializing in NestedContentParser", e);
                return null;
            }            
        }

        /// <summary>
        /// Split the list of dictionaries into separate dictionaries, and handle each dictionary.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="propertyDictionaries"></param>
        /// <param name="fields"></param>
        /// <param name="stopProcessing"></param>
        public void ProcessPropertyDictionaries(StringBuilder sb, List<Dictionary<string, object>> propertyDictionaries, 
            SearchField[] fields, bool stopProcessing) {

            foreach (var dictionary in propertyDictionaries)
            {
                AppendPropertyValues(sb, dictionary, fields, false);

            }
        }

        /// <summary>
        /// Extract the content of the nested content and append it to the Azure Search Index.
        /// </summary>
        /// <param name="sb">A string builder</param>
        /// <param name="dictionary">A dictionary of Umbraco content properties</param>
        /// <param name="fields">The configured list of fields/properties which may be added to the index</param>
        /// <param name="stopProcessing">Do not recurse</param>
        private void AppendPropertyValues(StringBuilder sb, Dictionary<string, object> dictionary,
            SearchField[] fields, bool stopProcessing)
        {
            foreach (var prop in dictionary)
            {
                var propValue = prop.Value?.ToString();

                if (string.IsNullOrEmpty(propValue))
                {
                    continue;
                }
                    
                if ( propValue.StartsWith("[")) // The property is itself nested content.
                {
                    try
                    {
                        var nestedSections = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(propValue);

                        if (!stopProcessing)
                        {
                            ProcessPropertyDictionaries(sb, nestedSections, fields, false);
                        }                            
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error in AppendPropertyValues", e);
                    }
                }
                else
                {                        
                    var fieldConfig = fields.FirstOrDefault(x => x.Name == prop.Key);

                    if (fieldConfig == null) continue; // The field is not configured to be added to the index.

                    if (fieldConfig.IsNestedLink)
                    {
                        // A nested link is a type of nested content in which only the link to the content is nested, 
                        // while the actual content is a separate node elsewhere in the tree.
                        AddLinkedContent(prop, sb, fields);
                    }
                    else
                    {
                        var fieldContent = propValue.StripHtml();

                        sb.Append(fieldContent);
                        sb.Append(" ");
                    }
                }                    
            }            
        }

        /// <summary>
        /// Add content which is in a separate node elsewhere in the tree.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="sb"></param>
        /// <param name="fields"></param>
        private void AddLinkedContent(KeyValuePair<string, object> prop, StringBuilder sb, SearchField[] fields)
        {
            string propValue = prop.Value.ToString();

            if (string.IsNullOrEmpty(propValue))
            {
                return;                
            }

            // A list of id's of nodes to be nested.
            var idList = propValue.Split(',').Select(int.Parse).ToList();

            if (!idList.Any())
            {
                return;
            }

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

            foreach (var id in idList)
            {
                // Get the content.
                IPublishedContent content = umbracoHelper.TypedContent(id);

                // Get the properties and put them in a dictionary.
                var properties = content.Properties;

                Dictionary<string, object> propertyList = new Dictionary<string, object>();

                foreach (var publishedProperty in properties)
                {
                    propertyList.Add(publishedProperty.PropertyTypeAlias, publishedProperty.DataValue);
                }
                
                // Prevent endless loop because of circular references: set stopProcessing to true.
                AppendPropertyValues(sb, propertyList, fields, true);
            }
        }
    }
}