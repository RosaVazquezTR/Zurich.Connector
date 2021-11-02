using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.App.Utils;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    /// <summary>
    /// Service to help facilitate the connectors data call. This should only be doing the work to get data and transform request and response objects.
    /// </summary>
    public interface IConnectorDataService
    {
        /// <summary>
        /// Get the data mapping results for a connector
        /// </summary>
        /// <param name="connectorIdentifier">The id or alias of the connector</param>
        /// <param name="hostname">The domain of the api being called</param>
        /// <param name="transferToken">The transfer token to pass with the api call, if needed</param>
        /// <param name="queryParameters">The query string parameters of request</param>
        /// <returns>Mapped data for the connector</returns>
        Task<dynamic> GetConnectorData(string connectorIdentifier, string hostname, string transferToken, Dictionary<string, string> queryParameters, bool retrievefilters);
    }

    public class ConnectorDataService : IConnectorDataService
    {
        private readonly IDataMappingFactory _dataMappingFactory;
        private readonly IDataMappingRepository _dataMappingRepo;
        private readonly ICosmosService _cosmosService;
        private readonly ILogger<ConnectorService> _logger;
        private readonly IMapper _mapper;
        private readonly IDataMappingService _dataMappingService;
        private readonly IConnectorDataSourceOperationsFactory _dataSourceOperationsFactory;
        private readonly IRegistrationService _registrationService;
        private readonly IDataExtractionService _dataExtractionService;

        public ConnectorDataService(
            IDataMappingFactory dataMappingFactory,
            IDataMappingRepository dataMappingRepo,
            ILogger<ConnectorService> logger,
            IMapper mapper, ICosmosService cosmosService,
            IDataMappingService dataMappingService,
            IConnectorDataSourceOperationsFactory dataSourceOperationsFactory,
            IRegistrationService registrationService,
            IDataExtractionService dataExtractionService)
        {
            _dataMappingFactory = dataMappingFactory;
            _dataMappingRepo = dataMappingRepo;
            _cosmosService = cosmosService;
            _logger = logger;
            _mapper = mapper;
            _dataMappingService = dataMappingService;
            _dataSourceOperationsFactory = dataSourceOperationsFactory;
            _registrationService = registrationService;
            _dataExtractionService = dataExtractionService;
        }

        /// <summary>
        /// Get Connector Data
        /// </summary>
        /// <param name="connectionIdentifier">This can be connector id or alias</param>
        /// <param name="hostname"></param>
        /// <param name="transferToken"></param>
        /// <param name="queryParameters"></param>
        /// <param name="retrievefilters"></param>
        /// <returns>Connector data</returns>
        public async Task<dynamic> GetConnectorData(string connectionIdentifier, string hostname, string transferToken, Dictionary<string, string> queryParameters, bool retrieveFilters)
        {
            ConnectorModel connectorModel = await _dataMappingService.RetrieveProductInformationMap(connectionIdentifier, hostname, retrieveFilters);

            if (connectorModel == null)
            {
                return null;
            }

            NameValueCollection mappedQueryParameters = MapQueryParametersFromDB(queryParameters, connectorModel);
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);           
            Dictionary<string, string> headerParameters = await _dataExtractionService.ExtractDataSource(mappedQueryParameters, queryParameters, hostname, connectorDocument);
            IDataMapping service = _dataMappingFactory.GetImplementation(connectorModel?.DataSource?.SecurityDefinition?.Type);

            var data = await service.GetAndMapResults<dynamic>(connectorDocument, transferToken, mappedQueryParameters, headerParameters);
            data = await EnrichConnectorData(connectorModel, data);
            if (retrieveFilters == true)
            {
                JToken mappingFilters = JToken.FromObject(connectorDocument.Filters);
                data[Constants.filters] = mappingFilters;
            }
            return data;
        }

        public NameValueCollection MapQueryParametersFromDB(Dictionary<string, string> cdmQueryParameters, ConnectorModel connectorModel)
        {
            NameValueCollection modifiedQueryParameters = new NameValueCollection();
            var queryParameters = new Dictionary<string, string>();
            var sortParameters = new Dictionary<string, string>();

            if (cdmQueryParameters.Any())
            {
                if (connectorModel?.Pagination?.Available == true)
                {
                    cdmQueryParameters = SetupPagination(connectorModel, cdmQueryParameters);
                }

                if (connectorModel.Request?.Parameters != null)
                    queryParameters = (from param in cdmQueryParameters
                                       join requestParam in connectorModel.Request?.Parameters
                                       on param.Key.ToString().ToLower() equals requestParam.CdmName.ToLower()
                                       where requestParam.InClause != ODataConstants.OData && requestParam.InClause != InClauseConstants.Child
                                       select new { name = requestParam.Name, value = param.Value.ToString() }).ToDictionary(c => c.name, c => c.value);


                if (connectorModel.Request?.Sorting != null)
                    sortParameters = (from param in cdmQueryParameters
                                      join requestParam in connectorModel.Request?.Sorting?.Properties
                                      on param.Value.ToString().ToLower() equals requestParam.ElementValue.ToLower()
                                      select new { name = requestParam.Element, value = requestParam.ElementValue.ToString() })
                                  .ToDictionary(c => c.name, c => c.value);

            }

            if (ODataHandler.HasODataParams(connectorModel))
                ODataHandler.BuildQueryParams(cdmQueryParameters, connectorModel).ToList().ForEach(param => queryParameters.Add(param.Key, param.Value));

            // Add default parameters if not present in the request. ex: locale, ResultSize etc
            var defaultParameters = connectorModel.Request?.Parameters?.Where(t => DefaultParametersCheck(t, queryParameters))
                                .ToDictionary(c => c.Name, c => c.DefaultValue);
            
            IEnumerable<KeyValuePair<string, string>> allParameters = new Dictionary<string, string>();

            if (queryParameters.Any())
                allParameters = allParameters.Concat(queryParameters);

            if (defaultParameters != null && defaultParameters.Any())
                allParameters = allParameters.Concat(defaultParameters);

            if (sortParameters.Any())
                allParameters = allParameters.Concat(sortParameters);

            if (allParameters != null)
            {
                foreach (var parameter in allParameters)
                {
                    modifiedQueryParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return modifiedQueryParameters;
        }
         
        private bool DefaultParametersCheck(ConnectorRequestParameterModel request, Dictionary<string, string> queryParameters)
        {
            return !String.IsNullOrWhiteSpace(request.DefaultValue) 
                && !queryParameters.ContainsKey(request.Name) 
                && request.InClause != ODataConstants.OData
                && request.InClause != InClauseConstants.Headers;
        }
     
        private Dictionary<string, string> SetupPagination(ConnectorModel connectorModel, Dictionary<string, string> cdmQueryParameters)
        {
            // Ex: Office 365 uses 0 based offset numbering.
            if (connectorModel.Pagination?.IsZeroBasedOffset.HasValue == true && 
                connectorModel.Pagination?.IsZeroBasedOffset == false && 
                cdmQueryParameters.ContainsKey("Offset"))
            {
                cdmQueryParameters["Offset"] = (int.Parse(cdmQueryParameters["Offset"]) + 1).ToString();
            }
            return cdmQueryParameters;
        }

        /// <summary>
        /// Enriches the connector data using the app's corresponding data source operations service
        /// </summary>
        /// <param name="connector">The data connector</param>
        /// <param name="data">The entity data</param>
        /// <returns></returns>
        private async Task<dynamic> EnrichConnectorData(ConnectorModel connector, dynamic data)
        {
            var dataSourceOperationsService = _dataSourceOperationsFactory.GetDataSourceOperationsService(connector?.DataSource?.AppCode);
            if (dataSourceOperationsService != null)
                data = await dataSourceOperationsService.SetItemLink(connector.Info.EntityType, data, connector?.DataSource?.AppCode, connector.HostName);
            else
                _logger.LogInformation("No data source operations service found for {appCode}", connector?.DataSource?.AppCode ?? "");
            return data;
        }
    }
}
