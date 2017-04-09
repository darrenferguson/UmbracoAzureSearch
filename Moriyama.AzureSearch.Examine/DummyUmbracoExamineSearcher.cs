using Examine;
using Examine.SearchCriteria;
using System.IO;
using UmbracoExamine;

namespace Moriyama.AzureSearch.Examine
{
    public class DummyUmbracoExamineSearcher : UmbracoExamineSearcher
    {
        public override ISearchResults Search(ISearchCriteria searchParams)
        {
            // var s = searchParams.ToString();
           
            throw new FileNotFoundException("");           
        }
    }
}
