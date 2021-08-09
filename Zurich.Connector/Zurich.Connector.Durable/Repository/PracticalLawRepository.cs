using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Zurich.Common.Models.OAuth;
using Zurich.Connector.Durable.Constants;
using Zurich.Connector.Durable.Model;

namespace Zurich.Connector.Durable.Repository
{
	/// <summary>
	/// Practical Law Repository
	/// </summary>
	public interface IPracticalLawRepository
    {
		/// <summary>
		/// Get the taxonomy list based on the locale.
		/// </summary>
		/// <param name="token"> PL Token</param>
		/// <param name="locale"> Locale </param>
		/// <param name="facet"> Facet </param>
		/// <returns></returns>
		public Task<ServicePracticeAreaList> GetTaxonomyList(string token, string locale, PracticalLawFacet facet);

	}

	public class PracticalLawRepository: IPracticalLawRepository
	{
		private readonly HttpClient _httpClient;
		private readonly OAuthConnection _oAuthConnection;
		

		public PracticalLawRepository(HttpClient httpClient, OAuthOptions oAuthOptions)
        {
			_httpClient = httpClient;
			_oAuthConnection = oAuthOptions.Connections[PlConstants.PLServiceAppCode];
		}

		/// <summary>
		/// Get the taxonomy list based on the provided locale.
		/// </summary>
		/// <param name="token"></param>
		/// <param name="locale"></param>
		/// <param name="facet"></param>
		/// <returns></returns>
		public async Task<ServicePracticeAreaList> GetTaxonomyList(string token, string locale, PracticalLawFacet facet)
		{
			UriBuilder uriBuilder = new UriBuilder($"{_oAuthConnection.BaseUrl}{PlConstants.TaxonomyEndpoint}");
			NameValueCollection parameters = HttpUtility.ParseQueryString(uriBuilder.Query);
			ServicePracticeAreaList practiceAreaList = new ServicePracticeAreaList();

			parameters.Add(PlConstants.Locale, locale); ;
			parameters.Add(PlConstants.Facet, facet.ToString());
			uriBuilder.Query = parameters.ToString();

			using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString()))
			{
				var httpContent = await MakePracticalLawAccessRequest(token, requestMessage);
				practiceAreaList = JsonConvert.DeserializeObject<ServicePracticeAreaList>(httpContent);

				return practiceAreaList;
			}

		}

		private async Task<string> MakePracticalLawAccessRequest(string token, HttpRequestMessage requestMessage)
		{
			if (string.IsNullOrWhiteSpace(token))
				return string.Empty;

			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			requestMessage.Headers.Add("accept", "application/json");

			var result = await _httpClient.SendAsync(requestMessage);

			if (!result.IsSuccessStatusCode)
			{
				System.Diagnostics.Trace.TraceError("Non Successful response from practical law.");
				return string.Empty;
			}

			var requestContent = await result.Content.ReadAsStringAsync();
			return requestContent;
		}
	}
}
