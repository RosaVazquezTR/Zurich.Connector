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
using Zurich.Connector.Data.Utils;
using Zurich.Connector.Data.Factories;
using System.Net;
using Zurich.Connector.Data.Interfaces;

namespace Zurich.Connector.Data.Repositories
{
    public class Repository(HttpClient httpClient, ILogger<Repository> logger, IAppConfigService appConfigService) : IRepository
    {
        public async Task<string> MakeRequest(ApiInformation apiInformation, NameValueCollection parameters, string body)
        {
            string response = apiInformation.Method.ToUpper() switch
            {
                "POST" => await Post(apiInformation, parameters, body),
                "GET" => await Get(apiInformation, parameters),
                _ => throw new NotImplementedException($"{apiInformation.Method} not currently implemented"),
            };

            return response;
        }


        public async Task<string> HandleSuccessResponse(string data, bool transformToPDF = true)
        {
            HashSet<string> supportedFormats = new() { "PDF", "DOC", "DOCX", "RTF" };
            
            byte[] byteArray = Convert.FromBase64String(data);
            await using MemoryStream documentStream = new MemoryStream(byteArray);

            string fileExtension = FileFormatParser.FindDocumentTypeFromStream(documentStream);

            if (!supportedFormats.Contains(fileExtension.ToUpper()))
            {
                throw new ApplicationException($"Unsupported document format. Supported formats are {string.Join(", ", supportedFormats)}");
            }

            try
            {
                IAsposeService asposeInstance = AsposeServiceFactory.GetAsposeImplementation(FileFormatParser.GetFileFormat(fileExtension));

                JObject document = asposeInstance.CreateDocumentJObject(documentStream, transformToPDF);

                return document.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error parsing document: {ex.Message}");
            }
        }

        public string HandleErrorResponse(HttpResponseMessage result, ApiInformation apiInformation, string uri)
        {
            var requestContent = result.Content.ReadAsStringAsync();
            switch (result.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    {
                        string message = $"{result.StatusCode} - Unable to find specified document from {apiInformation.AppCode}: {uri}";
                        logger.LogError("{message}", message);
                        throw new KeyNotFoundException(message);
                    }
                default:
                    {
                        string message = $"Non Successful response from {apiInformation.AppCode}\nSatuts Code: {(int)result.StatusCode} {result.StatusCode}\nURL:{uri}\nMessage: {requestContent.Result.ToString()}";
                        logger.LogError("{message}", message);
                        throw new ApplicationException(message);
                    }
            }
        }

        public async Task<string> Get(ApiInformation apiInformation, NameValueCollection parameters)
        {
            string uri = CreateUri(apiInformation, parameters);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            SetupRequestMessage(apiInformation, requestMessage);

            return await RetrieveResponse(apiInformation, requestMessage);
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

        private static string CreateUri(ApiInformation apiInformation, NameValueCollection parameters = null)
        {
            NameValueCollection queryString = ParseQueryString(apiInformation.UrlPath);

            if (parameters != null)
            {
                foreach (string key in parameters)
                {
                    queryString[key] = parameters[key];
                }
            }

            queryString.Remove("isFilterPlural");

            UriBuilder builder = new("https", apiInformation.HostName)
            {
                Path = GetPathWithoutQueryString(apiInformation.UrlPath),
                Query = ToQueryString(queryString)
            };

            return builder.Uri.ToString();
        }

        private static string GetPathWithoutQueryString(string urlPath)
        {
            int queryIndex = urlPath.IndexOf("?");

            return queryIndex > 0 ? urlPath[..queryIndex] : urlPath;
        }

        private static NameValueCollection ParseQueryString(string urlPath)
        {
            int queryIndex = urlPath.IndexOf("?");

            if (queryIndex > 0)
            {
                string queryString = urlPath[(queryIndex + 1)..];

                return HttpUtility.ParseQueryString(queryString);
            }

            return new NameValueCollection();
        }

        private static string ToQueryString(NameValueCollection queryString)
        {
            string[] array = (from key in queryString.AllKeys
                              from value in queryString.GetValues(key)
                              select $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(value)}").ToArray();

            return string.Join("&", array);
        }

        private static void SetupRequestMessage(ApiInformation apiInformation, HttpRequestMessage requestMessage)
        {
            AddAuthorizationHeader(apiInformation, requestMessage);
            SetAcceptHeader(requestMessage);
            AddCustomHeaders(apiInformation, requestMessage);
        }

        private static void AddAuthorizationHeader(ApiInformation apiInformation, HttpRequestMessage requestMessage)
        {
            if (string.IsNullOrWhiteSpace(apiInformation.Token?.AccessToken))
            {
                return;
            }

            string authHeader = !string.IsNullOrEmpty(apiInformation.AuthHeader)
                ? apiInformation.AuthHeader
                : "Authorization";

            string authValue = !string.IsNullOrEmpty(apiInformation.AuthHeader)
                ? apiInformation.Token.AccessToken
                : $"{apiInformation.Token.TokenType} {apiInformation.Token.AccessToken}";

            requestMessage.Headers.TryAddWithoutValidation(authHeader, authValue);
        }

        private static void SetAcceptHeader(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Accept.Clear();
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static void AddCustomHeaders(ApiInformation apiInformation, HttpRequestMessage requestMessage)
        {
            if (apiInformation.Headers == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> header in apiInformation.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        private async Task<string> RetrieveResponse(ApiInformation apiInformation, HttpRequestMessage requestMessage)
        {
            var result = await httpClient.SendAsync(requestMessage);

            //Feature flag to simulate error in conectors requests to test puroposes
            if (await appConfigService.IsDynamicFeatureEnabled(Features.SimulateErrorDatasource, apiInformation.AppCode))
            {
                result.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            else if (await appConfigService.IsDynamicFeatureEnabled(Features.SimulateTimeoutDatasource, apiInformation.AppCode))
            {
                result.StatusCode = System.Net.HttpStatusCode.GatewayTimeout;
            }

            if (result.IsSuccessStatusCode)
            {
                var requestContent = string.Empty;
                var contentType = result.Content.Headers.ContentType.MediaType;

                if (contentType == "application/json")
                {
                   requestContent = await result.Content.ReadAsStringAsync();
                }
                else
                {
                    var content = await result.Content.ReadAsByteArrayAsync();
                    requestContent = Convert.ToBase64String(content);
                }
                return requestContent;
            }
            else
            {
                return HandleErrorResponse(result, apiInformation, requestMessage.RequestUri.ToString());
            }
        }
    }
}
