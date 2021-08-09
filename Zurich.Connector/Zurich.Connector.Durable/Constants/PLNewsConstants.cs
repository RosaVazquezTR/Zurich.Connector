using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Durable.Constants
{
	/// <summary>
	/// Practical Law Constnats
	/// </summary>
	public static class PlConstants
	{
		/// <summary>
		/// PL Service App code
		/// </summary>
		public const string PLServiceAppCode = "PracticalLaw";
		/// <summary>
		/// Taxonomy end point
		/// </summary>
		public const string TaxonomyEndpoint = "/v3/taxonomy";
		/// <summary>
		/// Facet
		/// </summary>
		public const string Facet = "facet";
		/// <summary>
		/// Practical law area IDs
		/// </summary>
		public const string PracticeAreaIds = "practiceAreaIds";
		/// <summary>
		/// Practical law areas.
		/// </summary>
		public const string PracticeAreas = "practiceAreas";
		/// <summary>
		/// Author
		/// </summary>
		public static readonly Dictionary<string, string> Author = new Dictionary<string, string>()
		{
			{"uk",  "Practical Law UK"},
			{"us",  "Practical Law US"},
			{"ca",  "Practical Law CA"},
		};
		/// <summary>
		/// Locale
		/// </summary>
		public const string Locale = "locale";

	}
}
