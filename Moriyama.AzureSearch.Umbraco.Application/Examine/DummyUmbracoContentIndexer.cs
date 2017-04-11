using System.Collections.Specialized;
using System.Xml.Linq;
using UmbracoExamine;

namespace Moriyama.AzureSearch.Umbraco.Examine
{
    public class DummyUmbracoContentIndexer : UmbracoContentIndexer
    {

        
        public override void ReIndexNode(XElement node, string type)
        {

        }
        public override void DeleteFromIndex(string nodeId)
        {

        }

        protected override void PerformIndexAll(string type)
        {

        }

        protected override void AddSingleNodeToIndex(XElement node, string type)
        {
        }

        public override void RebuildIndex()
        {
        }

    }
}
