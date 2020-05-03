using System.Collections.Generic;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
	public class SearchIndex
	{
		public string Name { get; set; }
		public string DefaultScoringProfile { get; set; }

		public List<AzureItemBase> Fields { get; set; }
		public List<AzureItemBase> ScoringProfiles { get; set; }
		public List<AzureItemBase> Suggesters { get; set; }
	}
}
