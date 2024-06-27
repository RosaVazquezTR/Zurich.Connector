using AutoMapper;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zurich.Connector.App.Factories;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Utils;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.App.Services
{
    public interface IDataExtractionService
    {
        /// <summary>
        /// Extracts the datasource of a given user
        /// </summary>
        /// <param name="mappedQueryParameters">The mapped query parameters of request</param>
        /// <param name="queryParameters">The query string parameters of request</param>
        /// <param name="hostname">The domain of the api being called</param>
        /// <param name="connectorDocument"></param>
        /// <param name="domain">The domain of the api</param>
        /// <returns>Extracted data for the connector</returns>
        Task<Dictionary<string, string>> ExtractDataSource(NameValueCollection mappedQueryParameters, Dictionary<string, string> queryParameters, string hostname, ConnectorDocument connectorDocument);

        /// <summary>
        /// Extracts the header params of a given user
        /// </summary>
        /// <param name="cdmQueryParameters">The mapped query parameters of request</param>
        /// <param name="connectorDocument"></param>
        /// <returns>Extracted data for the connector</returns>
        Dictionary<string, string> ExtractParams(Dictionary<string, string> cdmQueryParameters, ConnectorDocument connectorDocument, string paramType);
    }
    public class DataExtractionService : IDataExtractionService
    {
        private readonly ICosmosService _cosmosService;
        private readonly IMapper _mapper;
        private readonly IDataMappingFactory _dataMappingFactory;


        public DataExtractionService(ICosmosService cosmosService, IMapper mapper, IDataMappingFactory dataMappingFactory)
        {
            _cosmosService = cosmosService;
            _mapper = mapper;
            _dataMappingFactory = dataMappingFactory;
        }

        public async Task<Dictionary<string, string>> ExtractDataSource(NameValueCollection mappedQueryParameters, Dictionary<string, string> queryParameters, string hostname, ConnectorDocument connectorDocument)
        {
            var headerParameters = ExtractParams(queryParameters, connectorDocument, InClauseConstants.Headers);
            var extractIds = headerParameters.Values.Append(connectorDocument.Request.EndpointPath);
            var connectorIds = ExtractIds(extractIds);

            var pathParameters = ExtractParams(queryParameters, connectorDocument, InClauseConstants.Path);

            if (connectorIds.Count() > 0)
            {
                var additionalInfo = await GetAdditionalInformation(hostname, connectorIds, queryParameters);
                connectorDocument.Request.EndpointPath = UpdateIdProperty(connectorDocument.Request.EndpointPath, additionalInfo);
                foreach (var header in headerParameters)
                {
                    headerParameters[header.Key] = UpdateIdProperty(header.Value, additionalInfo);
                }
            }

            if (pathParameters.Count() > 0)
                connectorDocument.Request.EndpointPath = UpdatePathParameter(connectorDocument.Request.EndpointPath, pathParameters);
 
            return headerParameters;
        }

        public string UpdateIdProperty(string property, Dictionary<string, JToken> connectorResponse)
        {
            string newProperty = property;
            // Find all areas that have { } in url. ie. /work/api/v2/customers/{UserInfo.customer_id}/documents
            var regexMatch = Regex.Match(property, @"{([^}]*)}");
            foreach (var capture in regexMatch.Captures)
            {
                string stringFormat = capture.ToString();
                string stringFormatTrimmed = stringFormat.Trim('{', '}');
                string[] splitString = stringFormatTrimmed.Split(".");
                if (splitString.Length > 1)
                {
                    string id = splitString.First();
                    JToken result = connectorResponse[id];
                    for (int i = 1; i < splitString.Count(); i++)
                    {
                        result = result[splitString[i]];
                    }
                    // Replace the url variable inside { }
                    string value = result.Value<string>();
                    newProperty = Regex.Replace(newProperty, stringFormat, value);
                }
            }
            return newProperty;
        }

        public string UpdatePathParameter(string property, Dictionary<string, string> pathParameters)
        {
            string newProperty = property;


            foreach (KeyValuePair<string,string> parameter in pathParameters)
            {
                string parameterEndPointPathKey = $"{{{parameter.Key}}}";

                // Replace the url variable inside { }
                if (pathParameters.TryGetValue(parameter.Key, out string value))
                {
                    newProperty = Regex.Replace(newProperty, parameterEndPointPathKey, value);
                }

            }
            return newProperty;
        }

        public IEnumerable<string> ExtractIds(IEnumerable<string> responses)
        {
            List<string> ids = new List<string>();
            foreach (var response in responses)
            {
                var splitString = SplitString(response);
                if (splitString?.Length > 1)
                {
                    string id = splitString.First();
                    ids.Add(id);
                }
            }
            return ids;
        }

        private string[] SplitString(string response)
        {
            var regexMatch = Regex.Match(response, @"{([^}]*)}");
            foreach (var capture in regexMatch.Captures)
            {
                //probably a better way to do this
                string stringFormat = capture.ToString();
                string stringFormatTrimmed = stringFormat.Trim('{', '}');
                string[] splitString = stringFormatTrimmed.Split(".");
                return splitString;
            }
            return null;
        }

        public Dictionary<string, string> ExtractParams(Dictionary<string, string> cdmQueryParameters, ConnectorDocument connectorDocument, string paramType)
        {
            if (connectorDocument.Request?.Parameters != null)
            {
                var qryParameters = (from param in cdmQueryParameters
                                        join requestParam in connectorDocument.Request?.Parameters
                                        on param.Key.ToString().ToLower() equals requestParam.CdmName.ToLower()
                                        where requestParam.InClause == paramType
                                     select new { name = requestParam.Name, value = param.Value.ToString() }).ToDictionary(c => c.name, c => c.value);
                var parameters = connectorDocument.Request.Parameters.Where(x => string.IsNullOrEmpty(x.CdmName) && x.InClause == paramType).Select(x => new { name = x.Name, value = x.DefaultValue.ToString() }).ToDictionary(c => c.name, c => c.value);
                return qryParameters.Concat(parameters).ToDictionary(x => x.Key, x => x.Value);
            }
            return null;
        }

        public async Task<Dictionary<string, JToken>> GetAdditionalInformation(string hostName, IEnumerable<string> ids, Dictionary<string, string> cdmQueryParameters)
        {
            var queryParameters = new Dictionary<string, string>();
            Dictionary<string, JToken> additionalInfo = new Dictionary<string, JToken>();
            var distinctIds = ids.Distinct();
            foreach (var id in distinctIds)
            {
                ConnectorModel connectorModel = await _cosmosService.GetConnector(id, true);
                ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
                if (!string.IsNullOrEmpty(hostName))
                {
                    connectorDocument.HostName = hostName;
                }

                NameValueCollection queryParams = new NameValueCollection();
                if (connectorDocument.Request.Parameters?.Count > 0)
                {
                    foreach (var param in connectorDocument.Request.Parameters)
                    {
                        if (param.InClause == ODataConstants.OData)
                        {
                            ODataHandler.BuildQueryParams(cdmQueryParameters, connectorModel).ToList().ForEach(param => queryParams.Add(param.Key, param.Value));
                        }
                        else
                        {
                            queryParams.Add(param.Name, cdmQueryParameters[param.CdmName]);

                        }
                    }
                }

                // Make api call to get the information for the url variable inside { }
                // Shouldn't be a need to pass transfer token, because after it is used it will be invalid
                IDataMapping service = _dataMappingFactory.GetImplementation(connectorModel?.DataSource?.SecurityDefinition?.Type);
                JToken result = await service.GetAndMapResults<JToken>(connectorDocument, null, queryParams, null, null);
                additionalInfo.Add(id, result);
            }
            return additionalInfo;
        }
    }
}
