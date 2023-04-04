using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.ProductData.Models;
using OAuthAPITokenResponse = Zurich.Common.Models.OAuth.OAuthAPITokenResponse;
using Zurich.Common;
using System.Net.Http;
using Newtonsoft.Json;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Exceptions;
using Zurich.TenantData;
using Zurich.Connector.Data.Utils;
using Zurich.Common.Models.FeatureFlags;
using Zurich.Common.Services;

namespace Zurich.Connector.Data.DataMap
{
    public abstract class AbstractDataMapping
    {
        protected IRepository _repository;
        protected IDataMappingRepository _dataMappingRepository;
        protected Type _objectType;
        protected IOAuthService _oAuthService;
        protected ILogger<AbstractDataMapping> _logger;
        protected ConnectorCosmosContext _cosmosContext;
        protected IMapper _mapper;
        protected IHttpBodyFactory _httpBodyFactory;
        protected IHttpResponseFactory _httpResponseFactory;
        protected IHttpContextAccessor _contextAccessor;
        protected IOAuthApiRepository _oAuthApirepository;
        protected OAuthOptions _oAuthOptions;
        protected ILegalHomeAccessCheck _legalHomeAccessCheck;
        protected IConfiguration _configuration;
        protected IAppConfigService _appConfigService;
        // Temporary measure to use the old way to obtain a token for HighQ, while highQ admin token is fixed in federated search
        // TODO: Remove this once the adminToken works in federated search and can be obtained from OAuth
        protected IHttpClientFactory _httpClientFactory;
        protected ISessionAccessor _sessionAccessor;

        public async virtual Task<T> GetAndMapResults<T>(ConnectorDocument dataTypeInformation, string transferToken, NameValueCollection query, Dictionary<string, string> headers, Dictionary<string, string> requestParameters, string domain = null)
        {
            T results = default(T);

            return results;
        }

        protected async Task<T> GetFromRepo<T>(ApiInformation apiInfo, ConnectorDocument connectorDocument, NameValueCollection query, Dictionary<string, string> requestParameter)
        {

            IHttpBodyService service = _httpBodyFactory.GetImplementation(apiInfo.Method);
            string body = service.CreateBody(connectorDocument, query);
            string response = await _repository.MakeRequest(apiInfo, query, body);

            if (!(string.IsNullOrWhiteSpace(response)))
            {
                IHttpResponseService httpResponseService = _httpResponseFactory.GetImplementation(connectorDocument.Response.Type.ToString());

                JToken jsonResponse = null;

                if (connectorDocument?.Response.UseJsonTransformation ?? false)
                    jsonResponse = await httpResponseService.GetJTokenResponse(response, connectorDocument.Response, connectorDocument.Id, requestParameter, query, _httpClientFactory);
                else
                    jsonResponse = await httpResponseService.GetJTokenResponse(response, connectorDocument.Response);

                if (httpResponseService.MapResponse)
                    return await MapToCDM<T>(jsonResponse, connectorDocument.ResultLocation, connectorDocument, requestParameter);
                else
                {
                    jsonResponse = await ModifyResult(jsonResponse, connectorDocument);
                    return jsonResponse.ToObject<T>();

                }
            }
            else
            {
                return default(T);
            }
        }

        private async Task<JToken> ModifyResult(JToken jsonResponse, ConnectorDocument connectorDocument)
        {
            if (jsonResponse["Documents"] != null)
            {
                JObject jobject = await MapProperties(connectorDocument.CdmMapping.structured, jsonResponse, null);
                foreach (var item in jobject)
                {
                    jsonResponse[item.Key] = item.Value.ToString();
                }
            }
            else
            {
                jsonResponse["Count"] = 0;
            }
            return jsonResponse;
        }

        public async virtual Task<OAuthAPITokenResponse> RetrieveToken(string appCode, OAuthApplicationType? appType = null,
                                                          string locale = null, string grandType = null, string productType = null, string domain = null)
        {
            // Temporary measure to use the old way to obtain a token for HighQ, while highQ admin token is fixed in federated search
            // TODO: Remove this once the adminToken works in federated search and can be obtained from OAuth
            if (appCode == Constants.AppCodes.HighQ)
            {
                await _sessionAccessor.PopulateUserInfo();
                AppToken token;
                // TODO: Modify this to obtain the instance name from the domain when more instances are available
                token = await GetImpersonatedUserToken(domain.Contains("collabdemo1") ? "collabdemo1" : "collabdemo2", _sessionAccessor.Email);

                // HighQ returned a lower case token type and it doesn't accept the token type being lower case in other calls so we have to capitalize it
                if (token != null)
                    token.token_type = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(token.token_type);

                var result = _mapper.Map<OAuthAPITokenResponse>(token);
                return result;
            }

            if (_legalHomeAccessCheck.isLegalHomeUser())
            {
                AppToken token;
                if (locale != null && grandType != null && appType.HasValue)
                {
                    token = await _oAuthService.RequestNewToken(appCode, grandType, appType.Value, locale: locale);
                }
                else
                {
                    token = await _oAuthService.GetToken(appCode, productType: string.IsNullOrEmpty(productType) ? null : new ProductType() { ProductTypeName = productType });
                }
                var result = _mapper.Map<OAuthAPITokenResponse>(token);
                return result;
            }
            else
            {
                OAuthAPITokenResponse result;
                // Making call to endpoint which takes AppCode and TenantId and it returns the token
                if (_sessionAccessor?.TenantId != Guid.Empty && _sessionAccessor.UserId == Guid.Empty)
                {
                    result = await _oAuthApirepository.GetTokenWithTenantId(appCode, _sessionAccessor.TenantId.ToString(), null);
                }
                else
                {
                    result = await _oAuthApirepository.GetToken(appCode);
                }

                return result;
            }
        }

        public async virtual Task<T> MapToCDM<T>(JToken jsonResponse, string resultLocation, ConnectorDocument connectorDocument, Dictionary<string, string> requestParameters)
        {
            // TODO: Remove this JToken logic when database is set up
            bool isJToken = typeof(T).Name == "JToken";

            if (!string.IsNullOrEmpty(resultLocation) || isJToken)
            {
                if (!string.IsNullOrEmpty(resultLocation))
                {
                    string[] resultsLocation = resultLocation.Split(".");
                    foreach (string location in resultsLocation)
                    {
                        if (jsonResponse is JArray)
                        {
                            if (connectorDocument.Info.SubType == "Parent")
                            {
                                // Default to first value in array at the moment if passed in the location
                                jsonResponse = jsonResponse.First();
                            }
                            else
                            {
                                // Mapping every value in the array at the moment if passed in a location
                                JArray results = new JArray();
                                foreach (var result in jsonResponse)
                                {
                                    results.Add(await PerformMapping(result, connectorDocument, requestParameters));
                                }
                                return (dynamic)results;
                            }
                        }
                        if (location != "[]")
                        {
                            jsonResponse = jsonResponse[location];
                        }
                    }
                }
                // Needed so we can prefetch api results that are needed in the original api call
                if (isJToken)
                {
                    // I can't just return a jToken... so bamboozle it with a dynamic
                    return (dynamic)jsonResponse;
                }
            }

            try
            {
                return await PerformMapping(jsonResponse, connectorDocument, requestParameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Deserialization Broke");
                //Should we throw the exception here?
            }

            return default(T);
        }

        /// <summary>
        /// Will make sure ApiInformation is setup correctly
        /// </summary>
        /// <param name="info">The apiInformation to check</param>
        public virtual void CleanUpApiInformation(ApiInformation info)
        {
            // can probably get rid of this when OAuth is setup
            if (string.IsNullOrEmpty(info.HostName))
            {
                if (_oAuthOptions.Connections.ContainsKey(info.AppCode))
                {
                    var appCodeBaseUrl = _oAuthOptions.Connections[info.AppCode].BaseUrl;
                    //because we use a url builder we need to drop the https, however we need this for the token information
                    info.HostName = UrlUtils.FormattingUrl(appCodeBaseUrl);
                }
            }
            else
            {
                info.HostName = UrlUtils.FormattingUrl(info.HostName);
            }
        }

        private async Task<dynamic> PerformMapping(JToken response, ConnectorDocument connectorDocument, Dictionary<string, string> requestParameters)
        {
            if (response == null)
            {
                return null;
            }
            if (response is JArray)
            {
                if (connectorDocument.Info.SubType == "Parent")
                {
                    return await MapResult(response, connectorDocument, requestParameters);
                }
                else
                {
                    JArray results = new JArray();
                    foreach (var result in response)
                    {
                        results.Add(await MapResult(result, connectorDocument, requestParameters));
                    }
                    return (dynamic)results;
                }

            }
            else if (response is JObject)
            {
                return await MapResult(response, connectorDocument, requestParameters);
            }
            else
            {
                throw new Exception("JSON response from API not recognized.");
            }
        }

        private async Task<dynamic> MapResult(dynamic apiResult, ConnectorDocument connector, Dictionary<string, string> requestParameters)
        {
            JObject cdmResult = new JObject();
            List<CDMElement> structuredProperties = connector?.CdmMapping?.structured;

            if (structuredProperties != null)
                cdmResult = await MapProperties(structuredProperties, apiResult, requestParameters);

            List<CDMElement> unstructuredProperties = connector?.CdmMapping?.unstructured;

            JObject additionalProps = new JObject();
            if (unstructuredProperties != null)
                additionalProps = await MapProperties(unstructuredProperties, apiResult, requestParameters);

            if (!cdmResult.ContainsKey(StructuredCDMProperties.AdditionalProperties))
                cdmResult[StructuredCDMProperties.AdditionalProperties] = additionalProps;

            return cdmResult;
        }

        private async Task<JObject> MapProperties(List<CDMElement> properties, dynamic apiResult, Dictionary<string, string> requestParameters)
        {
            JObject jObjectResult = new JObject();
            foreach (var property in properties)
            {
                JToken token = jObjectResult[property.name];
                // If result is already found for CDM property - skip mapping.
                if (token != null && token.Type != JTokenType.Null)
                    continue;
                // Get the correct json property when not on the same level
                string[] resultsLocation = property.responseElement.Split('.');
                if (!string.IsNullOrEmpty(property.type) && (property.type.Equals(DataTypes.InterpolationString, StringComparison.OrdinalIgnoreCase)))
                {
                    resultsLocation = new string[] { property.responseElement };
                }

                var tempResult = apiResult;
                foreach (string location in resultsLocation)
                {
                    // Perform new mapping recursively of array results
                    if (location.Contains('{'))
                    {
                        var match = Regex.Match(location, @"{(.*?)}");
                        if (match.Success)
                        {
                            var connectionId = match.Groups[1].ToString();
                            // TODO: Cache this datamapping so we don't have to call the DB for every result
                            var childConnector = await GetConnector(connectionId);

                            tempResult = await MapToCDM<dynamic>(tempResult,
                                                                    childConnector.ResultLocation,
                                                                    childConnector, requestParameters);
                        }
                    }
                    // Flatten array results instead 
                    else if (location.Contains('['))
                    {
                        var match = Regex.Match(location, @"\[(.*?):(.*?)\]");
                        if (match.Success)
                        {
                            string propertyName = match.Groups[1].ToString();
                            string valueToFind = match.Groups[2].ToString();
                            JArray resultArray = (JArray)tempResult;
                            tempResult = resultArray.FirstOrDefault(x => x[propertyName].ToString() == valueToFind);
                        }
                    }
                    else
                    {
                        tempResult = tempResult[location];
                        tempResult = ModifyResult(property, tempResult, requestParameters);
                    }
                    if (tempResult == null)
                        break;
                }

                jObjectResult[property.name] = tempResult;
            }

            return jObjectResult;
        }

        /// <summary>
        /// Modifies the returned results based on size.  If this starts to get big we should probably move it to its own class
        /// </summary>
        /// <param name="property">Current property from the CDM mapping in Cosmos</param>
        /// <param name="tempResult">The current result for the JObject</param>
        /// <param name="requestParameters">All the request parameters coming in</param>
        /// <returns></returns>
        private dynamic ModifyResult(CDMElement property, dynamic tempResult, Dictionary<string, string> requestParameters)
        {
            dynamic response = tempResult;
            DateTime startDate;

            if (!string.IsNullOrEmpty(property.type) && property.type.Equals(DataTypes.DateTime, StringComparison.OrdinalIgnoreCase))
            {
                if (property.format != null)
                {
                    if (DateTime.TryParseExact((string)response, property.format, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                        response = startDate;
                    else
                        response = default(DateTime);
                }
            }

            if (!string.IsNullOrEmpty(property.type) && property.type.Equals(DataTypes.Bool, StringComparison.OrdinalIgnoreCase))
            {
                bool.TryParse((string)response, out bool boolValue);
                response = boolValue;
            }
            else if (!string.IsNullOrEmpty(property.type) && property.type.Equals(DataTypes.InterpolationString, StringComparison.OrdinalIgnoreCase))
            {
                var propertyResponse = property.responseElement;
                var matches = Regex.Matches(propertyResponse, @"\((%*[\w()]+)\)");
                if (matches.Any())
                {
                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count >= 2)
                        {
                            string propertyName = match.Groups[1].Value.Replace("%", "");
                            string value = string.Empty;
                            // TODO: This should probably go elsewhere, but at the moment not sure where else to put it.
                            if (propertyName.Equals("NewGuid()", StringComparison.OrdinalIgnoreCase))
                            {
                                value = Guid.NewGuid().ToString();
                            }
                            else
                            {
                                if (requestParameters != null && requestParameters.ContainsKey(propertyName))
                                    value = requestParameters[propertyName];
                                else
                                    value = _configuration.GetValue<string>(propertyName);
                            }
                            if (match.Groups[1].Value.StartsWith("%"))
                            {
                                value = HttpUtility.UrlEncode(value);
                            }
                            propertyResponse = propertyResponse.Replace(match.Groups[0].Value, value);
                        }
                    }
                }
                response = propertyResponse;
            }

            return response;
        }

        /// <summary>
        /// Needed to get connectors for children connectors
        /// </summary>
        /// <param name="connectionId">The id of the connector the will be pulled from cosmos</param>
        /// <returns>Connector document from cosmos</returns>
        private async Task<ConnectorDocument> GetConnector(string connectionId)
        {
            var connectorDocument = await _cosmosContext.GetDocument<ConnectorDocument>
                                        (CosmosConstants.ConnectorContainerId, connectionId, CosmosConstants.ConnectorPartitionKey);

            string dataSourceId = connectorDocument?.Info?.DataSourceId;

            var connectorDataSourceDocument = await _cosmosContext.GetDocument<DataSourceDocument>
                                        (CosmosConstants.DataSourceContainerId, dataSourceId, CosmosConstants.DataSourcePartitionKey);

            connectorDocument.DataSource = connectorDataSourceDocument;

            return connectorDocument;
        }

        // Temporary measure to use the old way to obtain a token for HighQ, while highQ admin token is fixed in federated search
        // TODO: Remove GetInstanceAdminToken and GetImpersonatedUserToken once the adminToken works in federated search and can be obtained from OAuth
        public async Task<AppToken> GetInstanceAdminToken(string instanceName)
        {
            var token = await _oAuthService.GetToken(instanceName, OAuthApplicationType.ServiceApp);

            return token;
        }

        // Temporary measure to use the old way to obtain a token for HighQ, while highQ admin token is fixed in federated search
        // TODO: Remove GetInstanceAdminToken and GetImpersonatedUserToken once the adminToken works in federated search and can be obtained from OAuth
        public async Task<AppToken> GetImpersonatedUserToken(string instanceName, string email)
        {
            var instanceConnection = await _oAuthService.GetOAuthConnection(OAuthApplicationType.ServiceApp, instanceName);

            if (instanceConnection != null)
            {
                var adminToken = await GetInstanceAdminToken(instanceName);
                if (!string.IsNullOrEmpty(adminToken?.access_token))
                {
                    var client = _httpClientFactory.CreateClient(HttpClientNames.HighQ);

                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{instanceConnection.BaseUrl}{HighQMicroserviceAuthEndpoints.ImpersonateEndpoint}?useremail={email}");
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken.access_token);
                    var response = await client.SendAsync(requestMessage);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    //Feature flag to simulate error in oauth requests to test puroposes
                    if (await _appConfigService.IsDynamicFeatureEnabled(Features.SimulateErrorOAuthDatasource, Constants.AppCodes.HighQ))
                    {
                        response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    }
                    else if (await _appConfigService.IsDynamicFeatureEnabled(Features.SimulateTimeoutOAuthDatasource, Constants.AppCodes.HighQ))
                    {
                        response.StatusCode = System.Net.HttpStatusCode.GatewayTimeout;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var userToken = JsonConvert.DeserializeObject<AppToken>(responseContent);

                        return userToken;
                    }
                    else
                    {
                        _logger.LogError($"{response.StatusCode} Non Successful response from highQ Impersonate token {requestMessage.RequestUri.Host}");

                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);

                            if (errorResponse.Summary.Contains(HighQ.InvalidEmail))
                            {
                                _logger.LogWarning($"User email was invalid for {instanceName}");
                                throw new InvalidUserException(errorResponse.Summary);
                            }
                        }

                        _logger.LogError($"Unable to retrieve user token for {instanceName}. Server returned {response.StatusCode}: {responseContent}");
                    }
                }
            }
            else
            {
                _logger.LogError($"Unable to retrieve user token. Invalid instance name {instanceName ?? ""}");
            }

            return null;
        }
    }
}
