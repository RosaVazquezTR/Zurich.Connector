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

namespace Zurich.Connector.Data.DataMap
{
	public abstract class DataMappingBase
	{
		protected IRepository _repository;
		protected IDataMappingRepository _dataMappingRepository;
		protected Type _objectType;
		protected IOAuthService _oAuthService;
		protected ILogger<DataMappingBase> _logger;

		public async virtual Task<T> Get<T>(DataMappingClass dataTypeInformation, string transferToken = null)
        {
            T results = default(T);

            return results;
        }

    protected async Task<T> GetFromRepo<T>(ApiInformation apiInfo, DataMappingClass dataTypeInformation, NameValueCollection query = null)
        {
			var response = await _repository.Get(apiInfo, query);

			if (!string.IsNullOrWhiteSpace(response))
			{
				return await MapToCDM<T>(response, dataTypeInformation.ResultLocation, dataTypeInformation.Mapping);
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

		public async virtual Task<DataMappingClass> RetrieveProductInformationMap(string connectionId, string hostname)
		{
			DataMappingClass dataTypeInformation = await _dataMappingRepository.GetMap(connectionId);
			if (dataTypeInformation != null)
            {
				dataTypeInformation.Api.Hostname = hostname;
            }
			return dataTypeInformation;
		}

		public async virtual Task<string> UpdateUrl(string urlPath, DataMappingClass dataTypeInformation, string transferToken = null)
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
					dataTypeInformation = await this.RetrieveProductInformationMap(id, dataTypeInformation.Api.Hostname);

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

		public async virtual Task<T> MapToCDM<T>(string stringResponse, string resultLocation, List<DataMappingProperty> propertyMap)
		{
			// TODO: Remove this JToken logic when database is set up
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

			try
			{
				return PerformMapping(stringResponse, propertyMap);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Deserialization Broke");
				//Should we throw the exception here?
			}

            return default(T);
		}

		private dynamic PerformMapping(string stringResponse, List<DataMappingProperty> propertyMap)
        {
			var response = JsonConvert.DeserializeObject<JToken>(stringResponse);
			if (response is JArray)
			{
				List<dynamic> results = new List<dynamic>();
				foreach (var result in response)
				{
					results.Add(MapResult(result, propertyMap));
				}
				return (dynamic)results;
			}
			else if (response is JObject)
			{
				return MapResult(response, propertyMap);
			}
			else
			{
				throw new Exception("JSON response from API not recognized.");
			}
		}

		private dynamic MapResult(dynamic apiResult, List<DataMappingProperty> properties)
        {
			dynamic cdmResult = new JObject();
			foreach (var property in properties)
			{
				if (cdmResult[property.CDMProperty] != null)
					continue;

				// Get the correct json property when not on the same level
				string[] resultsLocation = property.APIProperty.Split('.');
				var tempResult = apiResult;
				foreach (string location in resultsLocation)
				{
					// Check if result is part of an array
					if (location.Contains('['))
					{
						var match = Regex.Match(location, @"\[(.*?)\]");
						var index = match.Groups[1].ToString();
						tempResult = tempResult[int.Parse(index)];
                    } 
					else
                    {
						tempResult = tempResult[location];
                    }
					if (tempResult == null)
						break;
				}
				cdmResult[property.CDMProperty] = tempResult;
			}
			return cdmResult;
		}

	}
}
