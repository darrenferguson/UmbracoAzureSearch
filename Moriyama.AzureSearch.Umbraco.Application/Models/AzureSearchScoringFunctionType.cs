using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
	public enum AzureSearchScoringFunctionType
	{
		freshness,
		magnitude,
		distance,
		tag
	}
}
