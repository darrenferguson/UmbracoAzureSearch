using System;
using System.Linq;
using Moriyama.AzureSearch.Umbraco.Application.Interfaces;
using Moriyama.AzureSearch.Umbraco.Application.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Helper
{
    public class SearchFacetHelper
    {
        public static BoolSearchFacet AsBoolSearchFacet(ISearchFacet searchFacet)
        {
            var result = new BoolSearchFacet()
            {
                Name = searchFacet.Name
            };

            if (searchFacet.Items == null || !searchFacet.Items.Any())
            {
                return result;
            }

            if (searchFacet.Items.Count() > 2)
            {
                throw new Exception("It cannot be converted as BoolSearchFacet object");
            }

            foreach (var keyValuePair in searchFacet.Items)
            {
                bool temp;

                if (!bool.TryParse(keyValuePair.Key, out temp))
                {
                    throw new Exception("It cannot be converted as BoolSearchFacet object");
                }

                if (temp)
                {
                    result.TrueCount = keyValuePair.Value;
                }
                else
                {
                    result.FalseCount = keyValuePair.Value;
                }
            }

            return result;
        }
    }
}
