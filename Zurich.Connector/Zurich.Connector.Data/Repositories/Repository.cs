using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zurich.Connector.Data.Model;
using System.Collections.Specialized;
using System.Web;
using Zurich.Common.Services;
using Zurich.Common.Models.FeatureFlags;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.IO;
using PdfiumViewer;
using Zurich.Connector.Data.Utils;
using Zurich.Connector.Data.Factories;

namespace Zurich.Connector.Data.Repositories
{
    public class Repository : IRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Repository> _logger;
        private readonly IAppConfigService _appConfigService;

        public Repository(HttpClient httpClient, ILogger<Repository> logger, IAppConfigService appConfigService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _appConfigService = appConfigService;
        }

        public async Task<string> MakeRequest(ApiInformation apiInformation, NameValueCollection parameters, string body)
        {
            string response;

            switch (apiInformation.Method.ToUpper())
            {
                case "POST":
                    response = await this.Post(apiInformation, parameters, body);
                    break;
                case "GET":
                    response = await this.Get(apiInformation, parameters);
                    break;
                default:
                    throw new NotImplementedException($"{apiInformation.Method} not currently implemented");
            }
            return response;
        }

        public async Task<string> DocumentDownloadMakeRequest(ApiInformation apiInformation)
        {
            string uri = CreateUri(apiInformation, null);
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                SetupRequestMessage(apiInformation, requestMessage);
                var result = await _httpClient.SendAsync(requestMessage);
                if (result.IsSuccessStatusCode)
                {
                    var documentStream = await result.Content.ReadAsStreamAsync();
                    string text = "";
                    string base64String = "";
                    JObject document = new JObject();
                    JObject pageText = new JObject();

                    var fileExtension = FileFormatParser.FindDocumentTypeFromStream(documentStream);
                    var asposeInstance = AsposeServiceFactory.GetAsposeImplementation(FileFormatParser.GetFileFormat(fileExtension));

                    using (var memoryStream = new MemoryStream())
                    {
                        await documentStream.CopyToAsync(memoryStream);
                        byte[] pdfBytes = memoryStream.ToArray();

                        // Convert the byte array to a Base64 string
                        base64String = Convert.ToBase64String(pdfBytes);
                    }
                    try
                    {
                        using (documentStream)
                        {
                            pageText = asposeInstance.CreateJObject(documentStream);
                        }
                        
                        document.Add("documentContent", pageText);
                        document.Add("documentBase64", base64String);
                    }
                    catch (Exception ex)
                    {
                        pageText.Add("1", "");
                        document.Add("documentContent", pageText);
                        document.Add("documentBase64", base64String);
                    }
                    return document.ToString();
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError($"{result.StatusCode} - Unable to find specified document from {apiInformation.AppCode}: {uri}");
                    throw new KeyNotFoundException($"Unable to find specified document from {apiInformation.AppCode}");
                }
                else
                {
                    _logger.LogError($"{result.StatusCode} Non Successful response from {apiInformation.AppCode}: {requestMessage.RequestUri.Host}");
                    throw new ApplicationException($"Non Successful Response from {apiInformation.AppCode}");
                }
            }          
        }

        public async Task<string> Get(ApiInformation apiInformation, NameValueCollection parameters)
        {
            string uri = CreateUri(apiInformation, parameters);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                SetupRequestMessage(apiInformation, requestMessage);

                return await RetrieveResponse(apiInformation, requestMessage);
            }
        }

        public async Task<string> Post(ApiInformation apiInformation, NameValueCollection parameters, string postBody)
        {
            string uri = CreateUri(apiInformation, parameters);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                SetupRequestMessage(apiInformation, requestMessage);

                requestMessage.Content = new StringContent(postBody, System.Text.Encoding.UTF8, "application/json");

                return await RetrieveResponse(apiInformation, requestMessage);
            }
        }

        public async Task<string> Put(ApiInformation apiInformation)
        {
            throw new NotImplementedException("Put not currently implemented");
        }

        public async Task<string> Delete(ApiInformation apiInformation)
        {
            throw new NotImplementedException("Delete not currently implemented");
        }

        private static string CreateUri(ApiInformation apiInformation, NameValueCollection parameters)
        {
            // TODO: We need this to parse a query string from the relative url. 
            // Eventually the query params should be passed in and this could be removed.
            var index = apiInformation.UrlPath.IndexOf("?");
            string relativePath = index > 0 ? apiInformation.UrlPath.Substring(0, index) : apiInformation.UrlPath;
            string query = index > 0 ? apiInformation.UrlPath.Substring(index) : string.Empty;
            NameValueCollection paramCollection = HttpUtility.ParseQueryString(query);
            if (parameters != null)
                paramCollection.Add(parameters);
            string scheme = "https";

            UriBuilder builder = new UriBuilder(scheme, apiInformation.HostName, -1, relativePath);
            string paramStr = paramCollection.ToString();
            if (paramCollection.AllKeys.Contains("isFilterPlural"))
            {
                string[] strParamArray = paramStr.Split('&');


                for (int s = 0; s < strParamArray.Length; s++)
                {
                    if (strParamArray[s].Contains("%2b"))
                        strParamArray[s] = strParamArray[s].Replace("%2b", "+");

                    if (strParamArray[s].Contains("%2c") && !strParamArray[s].Contains("searchTerm"))
                    {
                        string filterName = strParamArray[s][..strParamArray[s].IndexOf("=")];
                        strParamArray[s] = strParamArray[s].Replace("%2c", $"&{filterName}=");
                    }
                    // Removing the isFilterPlural parameter from uri parameters since it is only used internally
                    else if (strParamArray[s].Contains("isFilterPlural"))
                    {
                        strParamArray[s] = "";
                    }
                }
                paramStr = string.Join("&", strParamArray);

            }
            builder.Query = paramStr;
            return builder.ToString();
        }

        private void SetupRequestMessage(ApiInformation apiInformation, HttpRequestMessage requestMessage)
        {
            if (!string.IsNullOrWhiteSpace(apiInformation.Token?.AccessToken))
            {
                // Should we move this?
                if (string.IsNullOrEmpty(apiInformation.AuthHeader))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(apiInformation.Token.TokenType, apiInformation.Token.AccessToken);
                }
                else
                {
                    requestMessage.Headers.Add(apiInformation.AuthHeader, apiInformation.Token.AccessToken);
                }
            }
            requestMessage.Headers.Add("accept", "application/json");

            if (apiInformation.Headers?.Count > 0)
            {
                foreach (var header in apiInformation.Headers.Keys)
                {
                    requestMessage.Headers.Add(header, apiInformation.Headers[header]);
                }
            }
        }

        private async Task<string> RetrieveResponse(ApiInformation apiInformation, HttpRequestMessage requestMessage)
        {
            var result = await _httpClient.SendAsync(requestMessage);

            //Feature flag to simulate error in conectors requests to test puroposes
            if (await _appConfigService.IsDynamicFeatureEnabled(Features.SimulateErrorDatasource, apiInformation.AppCode))
            {
                result.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            else if (await _appConfigService.IsDynamicFeatureEnabled(Features.SimulateTimeoutDatasource, apiInformation.AppCode))
            {
                result.StatusCode = System.Net.HttpStatusCode.GatewayTimeout;
            }

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
        }
    }
}
