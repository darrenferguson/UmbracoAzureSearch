using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{

	public class AzureSearchSuggester
	{
		public string Name { get; set; }
		public string SearchMode { get; set; }
		public IList<string> SourceFields { get; set; }

		public Suggester GetSuggester()
		{
			if (!Enum.TryParse<SuggesterSearchMode>(this.SearchMode, out SuggesterSearchMode suggesterSearchMode))
			{
				suggesterSearchMode = SuggesterSearchMode.AnalyzingInfixMatching;
			}
			return new Suggester()
			{
				Name = this.Name,
				SearchMode = suggesterSearchMode,
				SourceFields = this.SourceFields
			};
		}
	}	
	
}
