using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Durable.Model
{
	/// <summary>
	/// Practical Law - Taxonomy response model.
	/// </summary>
	public class PlTaxonomyResponse
	{
		/// <summary>
		/// Taxonomy items list
		/// </summary>
		public IEnumerable<TaxonomyOptions> Categories { get; set; }
	}

	/// <summary>
	/// Taxonomy item list.
	/// </summary>
	public class TaxonomyOptions
	{
		/// <summary>
		/// Filter Name
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Filter ID
		/// </summary>
		public string Id { get; set; }
	}
}
