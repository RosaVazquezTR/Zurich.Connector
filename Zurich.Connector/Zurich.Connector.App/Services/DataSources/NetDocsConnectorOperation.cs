using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return appCode == KnownDataSources.netDocs;
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
            if (allParameters["q"].Contains("InQueryFilter"))
            {
                string updatedQuery = allParameters["q"];
                IEnumerable<ConnectorRequestParameterModel> inQueryFilters = connector.Request?.Parameters
                    .Where(p => p.CdmName.Contains("InQueryFilter"));
                foreach (ConnectorRequestParameterModel filterParam in inQueryFilters)
                {
                    updatedQuery = updatedQuery.Replace(filterParam.CdmName, filterParam.Name);
                    allParameters.Remove(filterParam.Name);
                }
                allParameters["q"] = updatedQuery;
            }
            return allParameters;
        }

        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            return item;
        }

        /// <summary>
        /// Function that adds the InQueryFilters to the query string
        /// </summary>
        /// <param name="queryParameters">The query string parameters of the request</param>
        /// <returns>THe updated query with the InQueryFilters</returns>
        private Dictionary<string, string> AddFiltersToQuery(Dictionary<string, string> queryParameters)
        {
            //Dictionary<string, string> newQueryParameters = queryParameters;

            foreach (string key in queryParameters.Keys)
            {
                if (key.Contains("InQueryFilter"))
                    queryParameters["Query"] += $" ={key}({queryParameters[key]})";
            }
            
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
            if (appCode == KnownDataSources.oneDrive)
            {
                // 93 = NetDocs user info
                connectorModel = await _cosmosService.GetConnector("93", true);
            }
            
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
            // Make api call to get the information for the webUrl variable
            connectorDocument.HostName = hostName;
            JToken userProfileResponse = await _dataMapping.GetAndMapResults<JToken>(connectorDocument, string.Empty, null, null, null);
            return userProfileResponse["primaryCabinetId"].Value<string>();
        }
    }
}
