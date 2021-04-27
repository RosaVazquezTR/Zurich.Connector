using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Data.Repositories
{
	/// <summary>
	/// Very generic Repository that should be able to make all API calls
	/// </summary>
	public interface IRepository
	{
		/// <summary>
		/// Makes a GET call
		/// </summary>
		/// <param name="apiInformation">Holds information around how to make the api call</param>
		/// <returns>a string</returns>
		Task<string> Get(ApiInformation apiInformation, string transferToken);

		/// <summary>
		/// Makes a POST call
		/// </summary>
		/// <param name="apiInformation">Holds information around how to make the api call</param>
		/// <returns>a string</returns>
		Task<string> Post(ApiInformation apiInformation);

		/// <summary>
		/// Makes a PUT call
		/// </summary>
		/// <param name="apiInformation">Holds information around how to make the api call</param>
		/// <returns>a string</returns>
		Task<string> Put(ApiInformation apiInformation);

		/// <summary>
		/// Makes a DELETE call
		/// </summary>
		/// <param name="apiInformation">Holds information around how to make the api call</param>
		/// <returns>a string</returns>
		Task<string> Delete(ApiInformation apiInformation);
	}

	public class Repository : IRepository
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<Repository> _logger;

		public Repository(HttpClient httpClient, ILogger<Repository> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<string> Get(ApiInformation apiInformation, string transferToken)
		{
			//if (string.IsNullOrWhiteSpace(apiInformation.Token?.access_token))
			//{
			//	return string.Empty;
			//}

			// TODO: We need this to parse a query string from the relative url. 
			// Eventually the query params should be passed in and this could be removed.
			var index = apiInformation.UrlPath.IndexOf("?");
			string relativePath = index > 0 ? apiInformation.UrlPath.Substring(0, index) : apiInformation.UrlPath;
			string query = index > 0 ? apiInformation.UrlPath.Substring(index) : string.Empty;
			if (transferToken != null)
            {
				if (!string.IsNullOrEmpty(query))
                {
					query = $"{query}&transferToken={transferToken}";
                } else
                {
					query = $"?transferToken={transferToken}";
				}
            }

            UriBuilder builder = new UriBuilder("https", apiInformation.HostName, -1, relativePath, query);
			using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, builder.ToString()))
			{
				if (!string.IsNullOrWhiteSpace(apiInformation.Token?.access_token))
				{
					if (string.IsNullOrEmpty(apiInformation.AuthHeader))
					{
						requestMessage.Headers.Authorization = new AuthenticationHeaderValue(apiInformation.Token.token_type, apiInformation.Token.access_token);
					}
					else
					{
						requestMessage.Headers.Add(apiInformation.AuthHeader, apiInformation.Token.access_token);
					}
				}
                requestMessage.Headers.Add("accept", "application/json");

				var result = await _httpClient.SendAsync(requestMessage);

				if (result.IsSuccessStatusCode)
				{
					var requestContent = await result.Content.ReadAsStringAsync();
					return requestContent;
				}
				else
				{
					_logger.LogError($"{result.StatusCode} Non Successful response from {apiInformation.AppCode}: {requestMessage.RequestUri.Host}");
					throw new ApplicationException($"Non Successful Response from {apiInformation.AppCode}");
				}

				return string.Empty;
			}
		}

		public async Task<string> Post(ApiInformation apiInformation)
		{
			throw new NotImplementedException("Post not currently implemented");
		}

		public async Task<string> Put(ApiInformation apiInformation)
		{
			throw new NotImplementedException("Put not currently implemented");
		}

		public async Task<string> Delete(ApiInformation apiInformation)
		{
			throw new NotImplementedException("Delete not currently implemented");
		}
	}
}
