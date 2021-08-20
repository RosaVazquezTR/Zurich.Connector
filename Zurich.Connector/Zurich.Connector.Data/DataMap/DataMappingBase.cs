﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Serializer;
using System.Collections.Specialized;
using AutoMapper;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Common;
using Zurich.Common.Repositories.Cosmos;
using System.Xml;
using System.Dynamic;

namespace Zurich.Connector.Data.DataMap
{
	public abstract class DataMappingBase
	{
		protected IRepository _repository;
		protected IDataMappingRepository _dataMappingRepository;
		protected Type _objectType;
		protected IOAuthService _oAuthService;
		protected ILogger<DataMappingBase> _logger;
		protected ICosmosClientStore _cosmosClientStore;
		protected IMapper _mapper;

		public async virtual Task<T> Get<T>(ConnectorDocument dataTypeInformation, string transferToken = null, NameValueCollection query = null)
		{
			T results = default(T);

			return results;
		}

		protected async Task<T> GetFromRepo<T>(ApiInformation apiInfo, ConnectorDocument dataTypeInformation, NameValueCollection query = null)
		{
			string response;

			switch (dataTypeInformation.request.method)
			{
				case "POST":
					string postBody = CreatePostBody(dataTypeInformation, query);
					response = await _repository.Post(apiInfo, postBody);
					break;
				default: //GET
					response = await _repository.Get(apiInfo, query);
					break;
			}

			if (!string.IsNullOrWhiteSpace(response))
			{
				return await MapToCDM<T>(response, dataTypeInformation.resultLocation, dataTypeInformation);
			}
			else
			{
				return default(T);
			}
		}

		public async virtual Task<AppToken> RetrieveToken(string appCode, OAuthApplicationType? appType = null, 
														  string locale = null, string grandType = null, bool? sendCredentialsInBody = false)
		{
			AppToken token;
			if (locale != null && grandType != null && appType.HasValue && sendCredentialsInBody.HasValue)
			{
				token = await _oAuthService.RequestNewToken(appCode, grandType, appType.Value, sendCredentialsInBody: sendCredentialsInBody.Value, locale: locale);
			}
			else
            {
				token = await _oAuthService.GetToken(appCode);
			}
			return token;
		}

		public async virtual Task<ConnectorDocument> RetrieveProductInformationMap(string connectionId, string hostname)
		{
			var connector = await GetConnector(connectionId);

			if (connector != null)
			{
				connector.hostName = hostname;
			}
			return connector;
		}

		public async virtual Task<string> UpdateUrl(string urlPath, ConnectorDocument dataTypeInformation, string transferToken = null)
		{
			string newUrlPath = urlPath;
			// Find all areas that have { } in url. ie. /work/api/v2/customers/{UserInfo.customer_id}/documents
			var regexMatch = Regex.Match(urlPath, @"{([^}]*)}");
			foreach (var capture in regexMatch.Captures)
			{
				//probably a better way to do this
				string stringFormat = capture.ToString();
				string stringFormatTrimmed = stringFormat.Trim('{', '}');
				string[] splitString = stringFormatTrimmed.Split(".");

				if (splitString.Length > 1)
				{
					string id = splitString.First();
					dataTypeInformation = await this.RetrieveProductInformationMap(id, dataTypeInformation.hostName);

					// Make api call to get the information for the url variable inside { }
					JToken result = await this.Get<JToken>(dataTypeInformation, transferToken);
					for (int i = 1; i < splitString.Count(); i++)
					{
						result = result[splitString[i]];
					}

					// Replace the url variable inside { }
					string value = result.Value<string>();
					newUrlPath = Regex.Replace(newUrlPath, stringFormat, value);
				}
			}
			return newUrlPath;
		}

		public async virtual Task<T> MapToCDM<T>(string stringResponse, string resultLocation, ConnectorDocument connectorDocument)
		{
			// TODO: Remove this JToken logic when database is set up
			bool isJToken = typeof(T).Name == "JToken";

			JToken jsonResponse = GetJsonResponse(stringResponse, connectorDocument.request);

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
						jsonResponse = jsonResponse[location];
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

        private JToken GetJsonResponse(string stringResponse, ConnectorRequest connectorRequest)
        {
			string jsonText = String.Empty;

			if (connectorRequest.responseContentType.HasValue && 
				connectorRequest.responseContentType.Value == ResponseContentType.XML && 
				stringResponse.StartsWith("<"))
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(stringResponse);
				if (!String.IsNullOrWhiteSpace(connectorRequest?.xmlArrayAttribute))
				{
					// if the search result has only one xml record, manually convert that into an array.
					xmlDoc = AddJsonArrayAttributes(xmlDoc, connectorRequest.xmlArrayAttribute);
				}
				jsonText = JsonConvert.SerializeXmlNode(xmlDoc);
			}
			else
			{
				jsonText = stringResponse;
			}

			return JToken.Parse(jsonText);
		}

		/// <summary>
		/// Convert any single element XML to a JSON Array by adding a tag "json:Array='true'".
		/// TODO - Replace this with any better solution.
		/// </summary>
		/// <param name="xmlArrayAttribute"></param>
		/// <param name="doc"></param>
		private XmlDocument AddJsonArrayAttributes(XmlDocument doc, string xmlArrayAttribute)
		{
			var elements = doc.SelectNodes(xmlArrayAttribute);
			if (elements != null && elements.Count == 1)
			{
				// Below namespaceURL is required to work the functionality.
				var jsonArray = doc.CreateAttribute("json", "Array", "http://james.newtonking.com/projects/json");
				jsonArray.Value = "true";
				(elements[0] as XmlElement).SetAttributeNode(jsonArray);
			}
			return doc;
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
            dynamic cdmResult = new JObject();
            List<CDMElement> properties = connector?.cdmMapping?.structured;

            await UpdateProperties(apiResult, cdmResult, properties);

			List<CDMElement> unstructuredProperties = connector?.cdmMapping?.unstructured;

			await UpdateProperties(apiResult, cdmResult, unstructuredProperties);
			return cdmResult;
        }

        private async Task UpdateProperties(dynamic apiResult, dynamic cdmResult, List<CDMElement> properties)
        {
            foreach (var property in properties)
            {
                if (cdmResult[property.name] != null)
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

                        tempResult = await MapToCDM<dynamic>(JsonConvert.SerializeObject(tempResult),
                                                                childConnector.resultLocation,
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

                cdmResult[property.name] = tempResult;
            }
        }

        private async Task<ConnectorDocument> GetConnector(string connectionId)
		{
			var connectorDocument = await _cosmosClientStore.GetDocument<ConnectorDocument>
										(CosmosConstants.ConnectorContainerId, connectionId, CosmosConstants.ConnectorPartitionKey);

			string dataSourceId = connectorDocument?.info?.dataSourceId;

			var connectorDataSourceDocument = await _cosmosClientStore.GetDocument<DataSourceDocument>
										(CosmosConstants.DataSourceContainerId, dataSourceId, CosmosConstants.DataSourcePartitionKey);

			connectorDocument.dataSource = connectorDataSourceDocument;

			return connectorDocument;
		}

		public virtual string CreatePostBody(ConnectorDocument connectorDocument, NameValueCollection parameters)
		{
			var requestMappingParameters = connectorDocument?.request?.parameters;
			if (requestMappingParameters == null)
			{
				return null;
			}

			JContainer JsonRequest = new JObject();
			var settings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge };

			var neededParams = requestMappingParameters.Where(x => parameters.AllKeys.Contains(x.name)).Select(y => new PostRequestParameter(y) { ParamValue = parameters[y.name] });
			foreach (var parameter in neededParams)
            {
				JTokenWriter writer = SetupPostJWriter(parameter);
				JsonRequest.Merge(writer.Token, settings);
			}
			return JsonRequest.ToString(Newtonsoft.Json.Formatting.None);
		}

		private JTokenWriter SetupPostJWriter(PostRequestParameter param)
		{
			JTokenWriter writer = new JTokenWriter();
			var parts = param.name.Split('.');
			WriteJsonObject(writer, parts, param.ParamValue);
			return writer;
		}

		private void WriteJsonObject(JTokenWriter writer, string[] paramParts, string value)
        {
			var paramPart = paramParts.First();
			var isArray = paramPart == "[]";

			if (isArray)
			{
				writer.WriteStartArray();
			}
			else
			{
				writer.WriteStartObject();
				writer.WritePropertyName(paramPart);
			}

			if (paramParts.Count() != 1)
				WriteJsonObject(writer, paramParts.Skip(1).ToArray(), value);
			else
			{
				var requestArray = value?.Split(',');
				if (requestArray != null)
				{
					foreach (var requestValue in requestArray)
					{
						writer.WriteValue(requestValue);
					}
				}
			}

			if (isArray)
				writer.WriteEndArray();
			else
				writer.WriteEndObject();
        }
	}
}
