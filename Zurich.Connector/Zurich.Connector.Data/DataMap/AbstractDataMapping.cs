using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.ProductData.Models;

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


		public async virtual Task<T> GetAndMapResults<T>(ConnectorDocument dataTypeInformation, string transferToken = null, NameValueCollection query = null, Dictionary<string, string> headers = null)
		{
			T results = default(T);

			return results;
		}

		protected async Task<T> GetFromRepo<T>(ApiInformation apiInfo, ConnectorDocument connectorDocument, NameValueCollection query = null)
		{
			IHttpBodyService service = _httpBodyFactory.GetImplementation(apiInfo.Method);
			string body = service.CreateBody(connectorDocument, query);
			string response = await _repository.MakeRequest(apiInfo, query, body);

			if (!string.IsNullOrWhiteSpace(response))
			{
				IHttpResponseService httpResponseService = _httpResponseFactory.GetImplementation(connectorDocument.Response.Type.ToString());
				JToken jsonResponse = httpResponseService.GetJTokenResponse(response, connectorDocument.Response);
				return await MapToCDM<T>(jsonResponse, connectorDocument.ResultLocation, connectorDocument);
			}
			else
			{
				return default(T);
			}
		}

		public async virtual Task<AppToken> RetrieveToken(string appCode, OAuthApplicationType? appType = null, 
														  string locale = null, string grandType = null, bool? sendCredentialsInBody = false, string productType = null)
		{
			AppToken token;
			if (_legalHomeAccessCheck.isLegalHomeUser())
            {
				if (locale != null && grandType != null && appType.HasValue && sendCredentialsInBody.HasValue)
				{
					token = await _oAuthService.RequestNewToken(appCode, grandType, appType.Value, sendCredentialsInBody: sendCredentialsInBody.Value, locale: locale);
				}
				else
				{
					token = await _oAuthService.GetToken(appCode, productType: string.IsNullOrEmpty(productType) ? null : new ProductType() { ProductTypeName = productType });
				}
				return token;
			}
            else
            {
				AppToken result = await _oAuthApirepository.GetToken(appCode);
				return result;

			}

		}

		public async virtual Task<T> MapToCDM<T>(JToken jsonResponse, string resultLocation, ConnectorDocument connectorDocument)
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
						if(jsonResponse is JArray)
                        {
							// Default to first value in array at the moment if passed in the location
							jsonResponse = jsonResponse.First();
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
				return await PerformMapping(jsonResponse, connectorDocument);
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
		public void CleanUpApiInformation(ApiInformation info)
        {
			// can probably get rid of this when OAuth is setup
			if (string.IsNullOrEmpty(info.HostName))
			{ 
				if (_oAuthOptions.Connections.ContainsKey(info.AppCode))
				{
					var appCodeBaseUrl = _oAuthOptions.Connections[info.AppCode].BaseUrl;
					//because we use a url builder we need to drop the https, however we need this for the token information
					info.HostName = appCodeBaseUrl.Replace("https://", "");
				}
			}
		}

		private async Task<dynamic> PerformMapping(JToken response, ConnectorDocument connectorDocument)
		{
			if (response == null)
			{
				return null;
			}
			if (response is JArray)
			{
				JArray results = new JArray();
				foreach (var result in response)
				{
					results.Add(await MapResult(result, connectorDocument));
				}
				return (dynamic)results;
			}
			else if (response is JObject)
			{
				return await MapResult(response, connectorDocument);
			}
			else
			{
				throw new Exception("JSON response from API not recognized.");
			}
		}

		private async Task<dynamic> MapResult(dynamic apiResult, ConnectorDocument connector)
        {
			JObject cdmResult = new JObject();
			List<CDMElement> structuredProperties = connector?.CdmMapping?.structured;

			if (structuredProperties != null)
				cdmResult = await MapProperties(structuredProperties, apiResult);

			List<CDMElement> unstructuredProperties = connector?.CdmMapping?.unstructured;

			JObject additionalProps = new JObject();
			if (unstructuredProperties != null)
				additionalProps = await MapProperties(unstructuredProperties, apiResult);

			if (!cdmResult.ContainsKey(StructuredCDMProperties.AdditionalProperties))
				cdmResult[StructuredCDMProperties.AdditionalProperties] = additionalProps;

			return cdmResult;
		}

		private async Task<JObject> MapProperties(List<CDMElement> properties, dynamic apiResult)
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

				var tempResult = apiResult;
				foreach (string location in resultsLocation)
				{
					// Perform new mapping recursively of array results
					if (location.Contains('{'))
					{
						var match = Regex.Match(location, @"{(.*?)}");
						var connectionId = match.Groups[1].ToString();
						// TODO: Cache this datamapping so we don't have to call the DB for every result
						var childConnector = await GetConnector(connectionId);

						tempResult = await MapToCDM<dynamic>(tempResult,
																childConnector.ResultLocation,
																childConnector);
					}
					// Flatten array results instead 
					else if (location.Contains('['))
					{
						var match = Regex.Match(location, @"\[(.*?):(.*?)\]");
						string propertyName = match.Groups[1].ToString();
						string valueToFind = match.Groups[2].ToString();
						JArray resultArray = (JArray)tempResult;
						tempResult = resultArray.FirstOrDefault(x => x[propertyName].ToString() == valueToFind);
					}
					else
					{
						tempResult = tempResult[location];
					}
					if (tempResult == null)
						break;
				}

				jObjectResult[property.name] = tempResult;
			}

			return jObjectResult;
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

	}
}
