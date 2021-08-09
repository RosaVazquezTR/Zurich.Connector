using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Durable.Model
{
	/// <summary>
	/// Practical Law filter model.
	/// </summary>
	public class PlFacetCommonModel
	{
		public PlFacetCommonModel()
		{
			this.ServicePracticeAreaList = new PlTaxonomyResponse();
		}
		/// <summary>
		/// /// Practical Law service practice area list.
		/// </summary>
		public PlTaxonomyResponse ServicePracticeAreaList { get; set; }
	}
}
