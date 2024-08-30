using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Connector.Data.Model;

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

        /// <summary>
        /// Calls oAuth to lists out the registrations for a user that can be registered
        /// </summary>
        /// <returns>returns the information around the domain, appcode and base url</returns>
        Task<List<DataSourceInformation>> GetAvailableRegistrations();


        /// <summary>
        /// Calls oAuth to Get a users current dataSource registrations
        /// </summary>
        /// <returns>returns the information around the domain, appcode and base url for user registrations</returns>
        Task<IEnumerable<DataSourceInformation>> GetUserRegistrations();


        /// <summary>
        /// Calls oAuth to get an appCodes corresponding authorize endpoint so the user can sign in
        /// </summary>
        /// <param name="applicationCode">The application to get the authorize url for</param>
        /// <returns>Authorize url</returns>
        Task<AuthorizeUrlResponse> GetAuthorizeUrl(string applicationCode);

        /// <summary>
        /// Revoke client Id and Secret for the appropriate connector
        /// </summary>
        /// <param name="applicationCode">The application code of the connector</param>
        /// <returns>Revoking of Tenanat application success or failure status</returns>
        Task<bool> RevokeTenantApplication(string applicationCode);
        /// <summary>
        /// Calls OAuth to Get the current tenant registration for a given AppCode
        /// </summary>
        /// <param name="applicationCode">The application code of the connector</param>
        /// <returns>Datasource tenant information</returns>
        Task<DataSourceTenantInformation> GetDatasourceTenantInfo(string appCode);
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
                string path = $"api/v1/{appCode}";
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
                {
                    var httpContent = await MakeRequest(requestMessage);

                    result = JsonConvert.DeserializeObject(httpContent);
                }
            }
            if (result == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<DataSourceInformation>> GetAvailableRegistrations()
        {
            List<DataSourceInformation> result;
            string path = $"api/v1/datasources";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, path))
            {
                var httpContent = await MakeRequest(requestMessage);

                result = JsonConvert.DeserializeObject<List<DataSourceInformation>>(httpContent);
            }

            return result;
        }

        public async Task<DataSourceTenantInformation> GetDatasourceTenantInfo(string applicationCode)
        {
            DataSourceTenantInformation result;
            string path = $"api/v1/datasources/{applicationCode}";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, path))
            {
                var httpContent = await MakeRequest(requestMessage);

                result = JsonConvert.DeserializeObject<DataSourceTenantInformation>(httpContent);
            }

            return result;
        }

        public async Task<IEnumerable<DataSourceInformation>> GetUserRegistrations()
        {
            string path = $"api/v1/datasources/me";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            var httpContent = await MakeRequest(requestMessage);
            return JsonConvert.DeserializeObject<IEnumerable<DataSourceInformation>>(httpContent);
        }

        public async Task<AuthorizeUrlResponse> GetAuthorizeUrl(string applicationCode)
        {
            AuthorizeUrlResponse result;
            string path = $"api/v1/{applicationCode}/authorizeURL";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, path))
            {
                var httpContent = await MakeRequest(requestMessage);

                result = JsonConvert.DeserializeObject<AuthorizeUrlResponse>(httpContent);
            }

            return result;
        }

        public async Task<bool> RevokeTenantApplication(string applicationCode)
        {
            bool response = false;
            string path = $"api/v1/{applicationCode}/all";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, path))
            {
                try
                {
                    var result = await _httpClient.SendAsync(requestMessage);
                    var requestContent = await result.Content.ReadAsStringAsync();

                    response = result.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unable to retrieve data. Server returned: {message}", ex.Message);
                    response = false;
                }
            }
            return response;
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
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return string.Empty;
                }
                _logger.LogError("Unable to retrieve data. Server returned: {code} - {message}", result.StatusCode.ToString(), requestContent ?? "");
                throw new ApplicationException($"Connectors returned status code {result.StatusCode} - {requestContent}");
            }
        }
    }
}
