using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{

	public class AzureSearchScoringProfile
	{
		public string Name { get; set; }
		public AzureSearchScoringTextWeights Text { get; set; }
		public List<AzureSearchScoringFunction> Functions { get; set; }

		public ScoringProfile GetScoringProfile()
		{
			return new ScoringProfile()
			{
				Name = this.Name,
				TextWeights = new TextWeights(this.Text.Weights),
				Functions = this.Functions?.Select(x => x.GetEffectiveScoringFunction()).ToList()
			};
		}
	}	
	
}
