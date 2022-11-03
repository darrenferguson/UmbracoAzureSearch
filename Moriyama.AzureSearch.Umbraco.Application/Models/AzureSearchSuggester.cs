using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{

    public class AzureSearchSuggester
    {
        public string Name { get; set; }
        public IList<string> SourceFields { get; set; }

        public Suggester GetSuggester()
        {
            
            return new Suggester()
            {
                Name = this.Name,
                SourceFields = this.SourceFields
            };
        }
    }

}
