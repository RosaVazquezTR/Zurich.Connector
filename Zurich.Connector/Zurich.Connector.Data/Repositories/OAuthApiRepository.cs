using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Common.Models.OAuth;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Data.Repositories
{
    public interface IOAuthApiRepository
    {
        /// <summary>
        /// Makes a Get call
        /// </summary>
        /// <param name="appCode">The partner app code</param>
        /// <returns>A <see cref="Token"/> contains information about the token including the access token, token type, and expires on information</returns>
        public Task<OAuthAPITokenResponse> GetToken(string appCode);

    }
    public class OAuthApiRepository : IOAuthApiRepository
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<OAuthApiRepository> _logger;

        public OAuthApiRepository(IHttpClientFactory httpClientFactory, ILogger<OAuthApiRepository> logger)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientNames.OAuthAPI);
            _logger = logger;
        }
        public async Task<OAuthAPITokenResponse> GetToken(string appCode)
        {
            dynamic token = "";
            if (appCode != null)
            {
                string path = $"api/v1/Token/{appCode}";
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, path))
                {
                    var httpContent = await MakeRequest(requestMessage);

                    token = JsonConvert.DeserializeObject<OAuthAPITokenResponse>(httpContent);
                }
                return token;
            }
            return null;
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
