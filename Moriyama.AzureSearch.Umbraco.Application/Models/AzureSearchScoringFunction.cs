using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moriyama.AzureSearch.Umbraco.Application.Models
{
	public class AzureSearchScoringFunction
	{
		public AzureSearchScoringFunctionType Type { get; set; }
		public string FieldName { get; set; }
		public int Boost { get; set; }
		public Nullable<ScoringFunctionInterpolation> Interpolation { get; set; }

		public Freshness Freshness { get; set; }
		public Magnitude Magnitude { get; set; }
		public Distance Distance { get; set; }
		public Tag Tag { get; set; }

		public ScoringFunction GetEffectiveScoringFunction()
		{
			switch (this.Type)
			{
				case AzureSearchScoringFunctionType.freshness:
					var fressnessParams = new FreshnessScoringParameters(TimeSpan.Parse(this.Freshness.BoostingDuration));
					return new FreshnessScoringFunction(this.FieldName, this.Boost, fressnessParams, this.Interpolation);
				case AzureSearchScoringFunctionType.magnitude:
					var magnitudeParams = new MagnitudeScoringParameters(this.Magnitude.BoostingRangeStart, this.Magnitude.BoostingRangeEnd, this.Magnitude.ConstantBoostBeyondRange);
					return new MagnitudeScoringFunction(this.FieldName, this.Boost, magnitudeParams, this.Interpolation);
				case AzureSearchScoringFunctionType.distance:
					var distanceParams = new DistanceScoringParameters(this.Distance.ReferencePointParameter, this.Distance.BoostingDistance);
					return new DistanceScoringFunction(this.FieldName, this.Boost, distanceParams, this.Interpolation);
				case AzureSearchScoringFunctionType.tag:
					var tagParams = new TagScoringParameters(this.Tag.TagsParameter);
					return new TagScoringFunction(this.FieldName, this.Boost, tagParams, this.Interpolation);
				default:
					throw new NotSupportedException($"{this.Type}");
			}
		}
	}

	#region Parameter Mapping Models

	public class Freshness
	{
		public string BoostingDuration { get; set; }

		/// <summary>
		/// TODO: This should be specified and converted from XSD "dayTimeDuration" format.
		/// https://docs.microsoft.com/en-us/rest/api/searchservice/add-scoring-profiles-to-a-search-index#bkmk_boostdur
		/// For now - parse as days.
		/// </summary>
		public TimeSpan BoostingDurationParsed
		{
			get
			{

                int.TryParse(this.BoostingDuration, out int parsedDuration);
				return TimeSpan.FromDays(parsedDuration);
			}
		}
	}

	public class Magnitude
	{
		public double BoostingRangeStart { get; set; }
		public double BoostingRangeEnd { get; set; }
		public Nullable<bool> ConstantBoostBeyondRange { get; set; }
	}

	public class Distance
	{
		public string ReferencePointParameter { get; set; }
		public double BoostingDistance { get; set; }
	}

	public class Tag
	{
		public string TagsParameter { get; set; }
	}


	#endregion
}
