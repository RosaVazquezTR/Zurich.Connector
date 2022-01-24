using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;

namespace Zurich.Connector.Data.Model
{
	/// <summary>
	/// Used to pass API information to the repository
	/// </summary>
	public class ApiInformation
	{
		/// <summary>
		/// The code used for the application we are calling
		/// </summary>
		public string AppCode { get; set; }

		/// <summary>
		/// The token that we will need to use
		/// </summary>
		public OAuthAPITokenResponse Token { get; set; }

		/// <summary>
		/// The Header that will be used for passing the token
		/// </summary>
		public string AuthHeader { get; set; }

		/// <summary>
		/// The Headers information
		/// </summary>
		public Dictionary<string, string> Headers { get; set; }
		/// <summary>
		/// The path of the url we are trying to call
		/// </summary>
		public string UrlPath { get; set; }

		/// <summary>
		/// The HostName of the product we are trying to call
		/// </summary>
		public string HostName { get; set; }

		/// <summary>
		/// Type of http method that the request will be.
		/// </summary>
		public string Method { get; set; }
	}
}
