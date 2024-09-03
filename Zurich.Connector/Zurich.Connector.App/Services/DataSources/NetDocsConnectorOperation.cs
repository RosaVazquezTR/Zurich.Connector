using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.App.Services.DataSources
{
    public class NetDocsConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;
        private readonly IConfiguration _configuration;
        private readonly ICosmosService _cosmosService;
        private readonly IMapper _mapper;

        public NetDocsConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IConfiguration configuration, ICosmosService cosmosService, IMapper mapper)
        {
            _logger = logger;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _configuration = configuration;
            _cosmosService = cosmosService;
            _mapper = mapper;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.netDocsUS;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Search:
                        if (item is Dictionary<string, string>)
                            item = AddFiltersToQuery(item);
                        else if (item is List<ConnectorsFiltersModel>)
                        {
                            JArray availableCabinets = await GetAvailableCabinets(appCode);
                            List<FilterList> filtersList = availableCabinets
                                .Select(cabinet => new FilterList
                                {
                                    Name = cabinet["cabinetName"].ToString(),
                                    Id = cabinet["cabinetId"].ToString()
                                })
                                .ToList();

                            item = filtersList;
                        }
                        break;
                }
            }
            catch (UriFormatException ex)
            {
                _logger.LogError("Unable to parse {entityType} web URL: {message}", entityType.ToString(), ex.Message ?? "");
            }
            return item;
        }

        public async Task<Dictionary<string, string>> SetParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            // Map the filters inQuery
            if (allParameters.ContainsKey("q") && allParameters["q"].Contains("InQueryFilter"))
            {
                StringBuilder updatedQuery = new(allParameters["q"]);
                IEnumerable<ConnectorRequestParameterModel> inQueryFilters = connector.Request?.Parameters
                    .Where(p => p.CdmName.Contains("InQueryFilter"));
                foreach (ConnectorRequestParameterModel filterParam in inQueryFilters)
                {
                    updatedQuery.Replace(filterParam.CdmName, filterParam.Name);
                    allParameters.Remove(filterParam.Name);
                }
                allParameters["q"] = updatedQuery.ToString();
            }
            return allParameters;
        }

        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            if (item is JObject searchResult && searchResult.ContainsKey("Folders") && searchResult["Folders"].HasValues)
            {
                foreach (var folder in searchResult["Folders"])
                {
                    if (folder["AdditionalProperties"] is JObject additionalProperties &&
                        additionalProperties.ContainsKey("parentId") &&
                        additionalProperties["parentId"] is JArray { Count: > 0 } parentIds)
                    {
                        additionalProperties["parentId"] = parentIds[0]["Id"].ToString().Replace("/", ":");
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// Function that adds the InQueryFilters to the query string
        /// </summary>
        /// <param name="queryParameters">The query string parameters of the request</param>
        /// <returns>The updated query with the InQueryFilters</returns>
        private Dictionary<string, string> AddFiltersToQuery(Dictionary<string, string> queryParameters)
        {
            StringBuilder updatedQuery = new(queryParameters["Query"]);
            foreach (string key in queryParameters.Keys.Where(k => k.Contains("InQueryFilter")))
            {
                string[] filterValues = queryParameters[key].Split(",");
                string filters = string.Join(" OR", filterValues.Select(value => $" ={key}({value})"));
                updatedQuery.Append(filters);
            }

            queryParameters["Query"] = updatedQuery.ToString();

            return queryParameters;
        }

        /// <summary>
        /// Gets the user's Primary Cabinet id from NetDocuments's user info
        /// </summary>
        /// <param name="appCode">The data source app code</param>
        /// <param name="hostName">The data source host name</param>
        private async Task<string> GetCabinetId(string appCode, string hostName)
        {
            ConnectorModel connectorModel = null;
            if (appCode == KnownDataSources.netDocsUS)
            {
                // 93 = NetDocs user info
                connectorModel = await _cosmosService.GetConnector("93", true);
            }

            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
            // Make api call to get the cabinet id from NetDocuments user information
            connectorDocument.HostName = hostName;
            JToken userProfileResponse = await _dataMapping.GetAndMapResults<JToken>(connectorDocument, string.Empty, null, null, null);
            return userProfileResponse["primaryCabinetId"].Value<string>();
        }

        /// <summary>
        /// Gets the available cabinets for the NetDocuments user
        /// </summary>
        /// <param name="appCode">The data source app code</param>
        private async Task<JArray> GetAvailableCabinets(string appCode)
        {
            ConnectorModel connectorModel = null;
            if (appCode == KnownDataSources.netDocsUS)
            {
                // 95 = NetDocs cabinets
                connectorModel = await _cosmosService.GetConnector("95", true);
            }

            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
            // Make api call to get the list of available cabinets from NetDocuments
            JArray userCabinetsResponse = await _dataMapping.GetAndMapResults<JArray>(connectorDocument, string.Empty, null, null, null);
            return userCabinetsResponse;
        }
    }
}
