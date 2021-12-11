using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Durable.Model
{
	/// <summary>
	/// PL Practice area list
	/// </summary>
	public class ServicePracticeAreaList
	{
		public IEnumerable<ServiceDef> Service { get; set; }
		public string SessionId { get; set; }
	}

	/// <summary>
	/// Service Definition
	/// </summary>
	public class ServiceDef
	{
		public string Name { get; set; }
		public IEnumerable<SiteName> SiteName { get; set; }
		public PlcReferenceTaxonomy Id { get; set; }
		public IEnumerable<Product> Product { get; set; }
		public IEnumerable<PracticeAreaList> PracticeAreaList { get; set; }
	}

	/// <summary>
	/// Practice Area List
	/// </summary>
	public class PracticeAreaList
	{
		public string Name { get; set; }
		public PlcReferenceTaxonomy Id { get; set; }
		[JsonProperty("practiceAreaList")]
		public IEnumerable<PracticeAreaListPA> PracticeAreaSubList { get; set; }
	}

	/// <summary>
	/// Practice Area List Properties
	/// </summary>
	public class PracticeAreaListPA
	{
		public string Name { get; set; }
		public PlcReferenceTaxonomy Id { get; set; }

	}

	/// <summary>
	/// PLC Reference taxonomy
	/// </summary>
	public class PlcReferenceTaxonomy
	{
		[JsonProperty("plcReference")]
		public string Value { get; set; }
	}

	/// <summary>
	/// Site Name
	/// </summary>
	public class SiteName
	{
		[JsonProperty("siteName")]
		public string SName { get; set; }

	}

	/// <summary>
	/// Product
	/// </summary>
	public class Product
	{
		[JsonProperty("product")]
		public string PName { get; set; }
	}
}
