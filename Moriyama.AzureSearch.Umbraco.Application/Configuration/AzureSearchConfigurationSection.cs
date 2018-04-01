using System;
using System.Configuration;

namespace Moriyama.AzureSearch.Umbraco.Application.Configuration
{
    public class AzureSearchConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("searchServiceName", IsRequired = true)]
        public string SearchServiceName
        {
            get
            {
                return (string) this["searchServiceName"];
            }
            set
            {
                this["searchServiceName"] = value;
            }

        }

        [ConfigurationProperty("searchServiceAdminApiKey", IsRequired = true)]
        public string SearchServiceAdminApiKey
        {
            get
            {
                return (string)this["searchServiceAdminApiKey"];
            }
            set
            {
                this["searchServiceAdminApiKey"] = value;
            }

        }

        [ConfigurationProperty("indexName", IsRequired = true)]
        public string IndexName
        {
            get
            {
                return (string)this["indexName"];
            }
            set
            {
                this["indexName"] = value;
            }
        }

        [ConfigurationProperty("indexBatchSize", IsRequired = false, DefaultValue = 500)]
        public int IndexBatchSize
        {
            get
            {
                return (int)this["indexBatchSize"];
            }
            set
            {
                this["indexBatchSize"] = value;
            }
        }

        [ConfigurationProperty("tempDirectory", IsRequired = false, DefaultValue = "")]
        public string TempDirectory
        {
            get
            {
                return (string)this["tempDirectory"];
            }
            set
            {
                this["tempDirectory"] = value;
            }
        }

        [ConfigurationProperty("fields")]
        public SearchFieldCollection Fields
        {
            get { return ((SearchFieldCollection)(base["fields"])); }
            set { base["fields"] = value; }
        }

    }

    public enum FieldTypeConfiguration
    {
        Int, String, Collection, Bool, Date
    }

    public class SearchFieldConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("type", DefaultValue = FieldTypeConfiguration.String, IsRequired = true)]
        public FieldTypeConfiguration FieldType
        {
            get
            {
                return (FieldTypeConfiguration)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        [ConfigurationProperty("isKey", DefaultValue = false, IsRequired = false)]
        public bool IsKey
        {
            get
            {
                return (bool)this["isKey"];
            }
            set
            {
                this["isKey"] = value;
            }
        }

        [ConfigurationProperty("isSortable", DefaultValue = false, IsRequired = false)]
        public bool IsSortable
        {
            get
            {
                return (bool)this["isSortable"];
            }
            set
            {
                this["isSortable"] = value;
            }
        }

        [ConfigurationProperty("isSearchable", DefaultValue = true, IsRequired = false)]
        public bool IsSearchable
        {
            get
            {
                return (bool)this["isSearchable"];
            }
            set
            {
                this["isSearchable"] = value;
            }
        }

        [ConfigurationProperty("isFacetable", DefaultValue = false, IsRequired = false)]
        public bool IsFacetable
        {
            get
            {
                return (bool)this["isFacetable"];
            }
            set
            {
                this["isFacetable"] = value;
            }
        }

        [ConfigurationProperty("isFilterable", DefaultValue = false, IsRequired = false)]
        public bool IsFilterable
        {
            get
            {
                return (bool)this["isFilterable"];
            }
            set
            {
                this["isFilterable"] = value;
            }
        }


        [ConfigurationProperty("isGridJson", DefaultValue = false, IsRequired = false)]
        public bool IsGridJson
        {
            get
            {
                return (bool)this["isGridJson"];
            }
            set
            {
                this["isGridJson"] = value;
            }
        }

    }


    public class SearchFieldCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "field";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }
        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName,
                StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SearchFieldConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SearchFieldConfiguration)(element)).Name;
        }

        public SearchFieldConfiguration this[string idx]
        {
            get { return (SearchFieldConfiguration)BaseGet(idx); }
        }
    }
}
