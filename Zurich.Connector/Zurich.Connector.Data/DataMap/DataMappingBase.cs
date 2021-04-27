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

namespace Zurich.Connector.Data.DataMap
{
	public abstract class DataMappingBase
	{
		protected IRepository _repository;
		protected IDataMappingRepository _dataMappingRepository;
		protected Type _objectType;
		protected IOAuthService _oAuthService;
		protected ILogger<DataMappingBase> _logger;

		public async virtual Task<T> Get<T>(string appCode, DataType dataType, DataMappingClass dataTypeInformation, string transferToken)
		{
			T results = default(T);

            //DataMappingClass dataTypeInformation = await this.RetrieveProductInformationMap(appCode, dataType); //move up a service

            if (dataTypeInformation == null)
			{
				return results;
			}
            //AppToken token = await this.RetrieveToken(appCode);
            //if (!string.IsNullOrEmpty(token?.access_token))
            //{
                // TODO: Grab the domains after Anu's change
                ApiInformation apiInfo = new ApiInformation() { AppCode = dataTypeInformation.AppCode, HostName = "us.practicallaw.qed.thomsonreuters.com", UrlPath = dataTypeInformation.Api.Url, AuthHeader = dataTypeInformation.Api.AuthHeader, Token = null };
                //ApiInformation apiInfo = new ApiInformation() { AppCode = dataTypeInformation.AppCode, HostName = "cloudiManage.com", UrlPath = dataTypeInformation.Api.Url, AuthHeader = dataTypeInformation.Api.AuthHeader, Token = token };
				apiInfo.UrlPath = await this.UpdateUrl(appCode, apiInfo.UrlPath, dataTypeInformation, transferToken);

				var response = await _repository.Get(apiInfo, transferToken);

				if (!string.IsNullOrWhiteSpace(response))
				{
					results = await MapToCDM<T>(response, dataTypeInformation.ResultLocation, dataTypeInformation.Mapping);
				}
            //}
            return results;
		}

		public async virtual Task<AppToken> RetrieveToken(string appCode)
		{
			AppToken token = await _oAuthService.GetToken(appCode);
			return token;
		}

		public async virtual Task<DataMappingClass> RetrieveProductInformationMap(string appCode, DataType dataType)
		{
			DataMappingClass dataTypeInformation = await _dataMappingRepository.GetMap(appCode, dataType);
			return dataTypeInformation;
		}

		public async virtual Task<string> UpdateUrl(string appCode, string urlPath, DataMappingClass dataTypeInformation, string transferToken)
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
				Enum.TryParse(splitString.First(), true, out DataType enumType);

				if (enumType != DataType.None)
				{
					dataTypeInformation = await this.RetrieveProductInformationMap(appCode, enumType);
					// Make api call to get the information for the url variable inside { }
					JToken result = await this.Get<JToken>(appCode, enumType, dataTypeInformation, transferToken);
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

		public async virtual Task<T> MapToCDM<T>(string stringResponse, string resultLocation, List<DataMappingProperty> propertyMap)
		{
			// Is this the best way to do this?
			bool isJToken = typeof(T).Name == "JToken";
			if (!string.IsNullOrEmpty(resultLocation) || isJToken)
			{
				JToken jToken = JToken.Parse(stringResponse);
				if (!string.IsNullOrEmpty(resultLocation))
				{
					string[] resultsLocation = resultLocation.Split(".");
					foreach (string location in resultsLocation)
					{
						jToken = jToken[location];
					}
				}
				// Needed so we can prefetch api results that are needed in the original api call
				if (isJToken)
				{
					// I can't just return a jToken... so bamboozle it with a dynamic
					return (dynamic)jToken;
				}
				stringResponse = jToken.ToString();
			}

			var jsonSettings = new JsonSerializerSettings();
			jsonSettings.ContractResolver = new CDMContractResolver(propertyMap);

			dynamic response;
			try
			{
				// what to do here if this is an object rather than an array
				response = JsonConvert.DeserializeObject<JToken>(stringResponse);
				if (response is JArray)
				{
					List<dynamic> results = new List<dynamic>();
					foreach (var result in response)
					{
						//dynamic stuff = new JObject();
						//foreach (var property in propertyMap)
						//{
						//	// Get the correct json property when not on the same level
						//	string[] resultsLocation = property.APIProperty.Split('.');
						//	var tempResult = result;
						//	foreach (string location in resultsLocation)
						//	{
						//		tempResult = tempResult[location];
						//	}

						//	stuff[property.CDMProperty] = tempResult;
						//}
						results.Add(MapResult(propertyMap, result));

					}
					return (dynamic)results;
				} else if (response is JObject)
                {
					return MapResult(propertyMap, resultLocation);
                } else
                {
					throw new Exception("JSON response from API not recognized.");
                }

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Deserialization Broke");
				//Should we throw the exception here?
			}

            return default(T);
		}

		private dynamic MapResult(List<DataMappingProperty> properties, dynamic apiResult)
        {
			dynamic cdmResult = new JObject();
			foreach (var property in properties)
			{
				// Get the correct json property when not on the same level
				string[] resultsLocation = property.APIProperty.Split('.');
				var tempResult = apiResult;
				foreach (string location in resultsLocation)
				{
					tempResult = tempResult[location];
				}

				cdmResult[property.CDMProperty] = tempResult;
			}
			return cdmResult;
		}

	}
}
