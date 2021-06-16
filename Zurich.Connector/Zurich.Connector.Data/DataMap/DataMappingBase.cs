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

namespace Zurich.Connector.Data.DataMap
{
	public abstract class DataMappingBase
	{
		protected IRepository _repository;
		protected IDataMappingRepository _dataMappingRepository;
		protected Type _objectType;
		protected IOAuthService _oAuthService;
		protected ILogger<DataMappingBase> _logger;
		protected ICosmosDocumentReader _cosmosDocumentReader;
		protected IMapper _mapper;

		public async virtual Task<T> Get<T>(ConnectorModelEntity dataTypeInformation, string transferToken = null, NameValueCollection query =null)
        {
            T results = default(T);

            return results;
        }

    protected async Task<T> GetFromRepo<T>(ApiInformation apiInfo, ConnectorModelEntity dataTypeInformation, NameValueCollection query = null)
        {
			var response = await _repository.Get(apiInfo, query);

			if (!string.IsNullOrWhiteSpace(response))
			{
				string resultLocation;
				resultLocation = dataTypeInformation?.CdmMappingEntity?.StructuredEntity?.CdmElementEntity?
									.Where(cdm => !string.IsNullOrEmpty(cdm.ResponseElement))
									.Select(cdm => cdm.ResponseElement).FirstOrDefault();

				return await MapToCDM<T>(response, resultLocation, dataTypeInformation);
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

		public async virtual Task<ConnectorModelEntity> RetrieveProductInformationMap(string connectionId, string hostname)
		{
			var dataTypeInformation = await GenerateConnectorModelEntity(connectionId);
			
			if (dataTypeInformation != null)
            {
				dataTypeInformation.HostName = hostname;
            }
			return dataTypeInformation;
		}

		public async virtual Task<string> UpdateUrl(string urlPath, ConnectorModelEntity dataTypeInformation, string transferToken = null)
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
					dataTypeInformation = await this.RetrieveProductInformationMap(id, dataTypeInformation.HostName);

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

		public async virtual Task<T> MapToCDM<T>(string stringResponse, string resultLocation, ConnectorModelEntity connectorModelEntity)
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
				return await PerformMapping(jTokenResponse, connectorModelEntity);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Deserialization Broke");
				//Should we throw the exception here?
			}

            return default(T);
		}

		private async Task<dynamic> PerformMapping(JToken response, ConnectorModelEntity connectorModelEntity)
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
					results.Add(await MapResult(result, connectorModelEntity));
				}
				return (dynamic)results;
			}
			else if (response is JObject)
			{
				return await MapResult (response, connectorModelEntity);
			}
			else
			{
				throw new Exception("JSON response from API not recognized.");
			}
		}

		private async Task<dynamic> MapResult(dynamic apiResult, ConnectorModelEntity connectorModelEntity)
		{
			dynamic cdmResult = new JObject();
			//List<DataMappingProperty> properties = connectorModelEntity.;
			List<CdmElementEntity> properties = connectorModelEntity?.CdmMappingEntity?.StructuredEntity?.CdmElementEntity;

			foreach (var property in properties)
			{
				if (cdmResult[property.Name] != null)
					continue;

				// Get the correct json property when not on the same level
				string responseElement = property.ResponseElement.LastIndexOf(".") > -1 ?
										 property.ResponseElement.Substring(property.ResponseElement.LastIndexOf(".") + 1 ) 
										 : property.ResponseElement;
				string[] resultsLocation = responseElement.Split('.');

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
						string resultLocation, responseElementEntity;
						responseElementEntity = dataMappingInformationEntity?.CdmMappingEntity?.StructuredEntity?.CdmElementEntity?
												.Where(cdm => !string.IsNullOrEmpty(cdm.ResponseElement))
												.Select(cdm => cdm.ResponseElement).FirstOrDefault();

						resultLocation = responseElementEntity.LastIndexOf(".") > -1 ?
										 responseElementEntity.Substring(responseElementEntity.LastIndexOf(".") + 1)
										 : responseElementEntity;

						tempResult = await MapToCDM<dynamic>(JsonConvert.SerializeObject(tempResult),
																resultLocation, 
																connectorModelEntity);
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

				cdmResult[property.Name] = tempResult;
			}
			return cdmResult;
		}

		private async Task<ConnectorModelEntity>GenerateConnectorModelEntity(string connectionId)
        {
			var connectorDocument = await _cosmosDocumentReader.GetConnectorDocument(connectionId);
			string dataSourceId = connectorDocument?.info?.dataSourceId;
			var connectorDataSourceDocument = await _cosmosDocumentReader.GetDataSourceDocument(dataSourceId);
			ConnectorModelEntity dataMappingInformationEntity = _mapper.Map<ConnectorModelEntity>(connectorDocument);
			DataSourceModelEntity dataSourceModelEntity = _mapper.Map<DataSourceModelEntity>(connectorDataSourceDocument);
			dataMappingInformationEntity.DataSource = dataSourceModelEntity;

			return dataMappingInformationEntity;
		}
	}
}
