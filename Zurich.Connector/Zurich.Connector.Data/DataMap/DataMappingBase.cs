using System;
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
using Zurich.Common.Repositories.Cosmos;

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
			var response = await _repository.Get(apiInfo, query);

			if (!string.IsNullOrWhiteSpace(response))
			{
				return await MapToCDM<T>(response, dataTypeInformation.resultLocation, dataTypeInformation);
			}
			else
			{
				return default(T);
			}
		}

		public async virtual Task<AppToken> RetrieveToken(string appCode)
		{
			AppToken token = await _oAuthService.GetToken(appCode);
			return token;
		}

		public async virtual Task<ConnectorDocument> RetrieveProductInformationMap(string connectionId, string hostname)
		{
			var dataTypeInformation = await GenerateConnectorModelEntity(connectionId);

			if (dataTypeInformation != null)
			{
				dataTypeInformation.hostName = hostname;
			}
			return dataTypeInformation;
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
			JToken jTokenResponse = JToken.Parse(stringResponse);
			if (!string.IsNullOrEmpty(resultLocation) || isJToken)
			{
				if (!string.IsNullOrEmpty(resultLocation))
				{
					string[] resultsLocation = resultLocation.Split(".");
					foreach (string location in resultsLocation)
					{
						jTokenResponse = jTokenResponse[location];
					}
				}
				// Needed so we can prefetch api results that are needed in the original api call
				if (isJToken)
				{
					// I can't just return a jToken... so bamboozle it with a dynamic
					return (dynamic)jTokenResponse;
				}
			}

			try
			{
				return await PerformMapping(jTokenResponse, connectorDocument);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Deserialization Broke");
				//Should we throw the exception here?
			}

			return default(T);
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

		private async Task<dynamic> MapResult(dynamic apiResult, ConnectorDocument connectorDocument)
		{
			dynamic cdmResult = new JObject();
			List<CDMElement> properties = connectorDocument?.cdmMapping?.structured;

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
						var dataMappingInformationEntity = await GenerateConnectorModelEntity(connectionId);

						tempResult = await MapToCDM<dynamic>(JsonConvert.SerializeObject(tempResult),
																dataMappingInformationEntity.resultLocation,
																dataMappingInformationEntity);
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
			return cdmResult;
		}

		private async Task<ConnectorDocument> GenerateConnectorModelEntity(string connectionId)
		{
			var connectorDocument = await _cosmosClientStore.GetDocument<ConnectorDocument>
										(CosmosConstants.ConnectorContainerId, connectionId, CosmosConstants.ConnectorPartitionKey);

			string dataSourceId = connectorDocument?.info?.dataSourceId;

			var connectorDataSourceDocument = await _cosmosClientStore.GetDocument<DataSourceDocument>
										(CosmosConstants.DataSourceContainerId, dataSourceId, CosmosConstants.DataSourcePartitionKey);

			connectorDocument.dataSource = connectorDataSourceDocument;

			return connectorDocument;
		}
	}
}
