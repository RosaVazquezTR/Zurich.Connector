using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common;

namespace Zurich.Connector.Data.Repositories
{
    /// <summary>
	/// This Repository  should be able to make all OAuth related API calls
	/// </summary>
    public interface IOAuthRepository
    {
        /// <summary>
        /// Makes a POST call
        /// </summary>
        /// <param name="appCode">Holds application code</param>
        /// <returns>a boolean value</returns>
        Task<bool> AutomaticRegistration(string appCode);
    }


    public class OAuthRepository : IOAuthRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OAuthRepository> _logger;
        public OAuthRepository(IHttpClientFactory httpClientFactory, ILogger<OAuthRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.OAuthAPI);
            _logger = logger;
        }
        public async Task<bool> AutomaticRegistration(string appCode)
        {
            dynamic result = "";
            if (appCode != null)
            {
                string path = $"oauth/api/v1/{appCode}";
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
                {
                    var httpContent = await MakeRequest(requestMessage);

                   result = JsonConvert.DeserializeObject(httpContent);
                }
            }
            if(result == null)
            { 
                return true; 
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Makes HTTP requests to a OAuth endpoint
        /// </summary>
        /// <param name="requestMessage">The request to send</param>
        /// <returns></returns>
        private async Task<string> MakeRequest(HttpRequestMessage requestMessage)
        {
            var result = await _httpClient.SendAsync(requestMessage);
            var requestContent = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
            {
                return requestContent;
            }
            else
            {
                _logger.LogError("Unable to retrieve data. Server returned: {code} - {message}", result.StatusCode.ToString(), requestContent ?? "");
                throw new ApplicationException($"Connectors returned status code {result.StatusCode} - {requestContent}");
            }
        }
    }
}
