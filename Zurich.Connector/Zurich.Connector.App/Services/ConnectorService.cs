﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Repositories;
using System.Collections.Specialized;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using System.Linq.Expressions;
using Zurich.Connector.Data.Model;
using Zurich.Connector.App.Services.DataSources;
using Newtonsoft.Json.Linq;
using Zurich.Connector.App;

namespace Zurich.Connector.Data.Services
{
    /// <summary>
    /// Service to help facilitate connector operation
    /// </summary>
    public interface IConnectorService
    {
        /// <summary>
        /// Get the data mapping results for a connector
        /// </summary>
        /// <param name="connectionId">The id of the connector</param>
        /// <param name="hostname">The domain of the api being called</param>
        /// <param name="transferToken">The transfer token to pass with the api call, if needed</param>
        /// <param name="queryParameters">The query string parameters of request</param>
        /// <returns>Mapped data for the connector</returns>
        Task<dynamic> GetConnectorData(string connectionId, string hostname, string transferToken, Dictionary<string, string> queryParameters, bool retrievefilters);

        /// <summary>
        /// Gets all connections from cosmos
        /// </summary>
        /// <param name="filters">Filters to get different connections</param>
        /// <returns>List of Data Mapping Connections <see cref="DataMappingConnection"/></returns>
        Task<List<ConnectorModel>> GetConnectors(ConnectorFilterModel filters);
        Task<ConnectorModel> GetConnector(string connectorId);

    }

    public class ConnectorService : IConnectorService
    {
        private readonly IDataMapping _dataMapping;
        private readonly IDataMappingFactory _dataMappingFactory;
        private readonly IDataMappingRepository _dataMappingRepo;
        private readonly ICosmosService _cosmosService;
        private readonly ILogger<ConnectorService> _logger;
        private readonly IMapper _mapper;
        private readonly IDataMappingService _dataMappingService;
        private readonly IConnectorDataSourceOperationsFactory _dataSourceOperationsFactory;

        public ConnectorService(IDataMapping dataMapping, IDataMappingFactory dataMappingFactory, IDataMappingRepository dataMappingRepo, 
                                ILogger<ConnectorService> logger, IMapper mapper, ICosmosService cosmosService, IDataMappingService dataMappingService, IConnectorDataSourceOperationsFactory dataSourceOperationsFactory)
        {
            _dataMapping = dataMapping;
            _dataMappingFactory = dataMappingFactory;
            _dataMappingRepo = dataMappingRepo;
            _cosmosService = cosmosService;
            _logger = logger;
            _mapper = mapper;
            _dataMappingService = dataMappingService;
            _dataSourceOperationsFactory = dataSourceOperationsFactory;
        }

        public async Task<dynamic> GetConnectorData(string connectionId, string hostname, string transferToken, Dictionary<string, string> queryParameters, bool retrievefilters)
        {
            ConnectorModel connectorModel = await _dataMappingService.RetrieveProductInformationMap(connectionId, hostname, retrievefilters);

            if (connectorModel == null)
            {
                return null;
            }

            NameValueCollection mappedQueryParameters;
            mappedQueryParameters = MapQueryParametersFromDB(queryParameters, connectorModel);

            Enum.TryParse(connectorModel?.DataSource?.SecurityDefinition?.Type, true, out AuthType outputEnum);

            IDataMapping service = _dataMappingFactory.GetMapper(outputEnum);

            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

            var data = await service.Get<dynamic>(connectorDocument, transferToken, mappedQueryParameters);
            data = EnrichConnectorData(connectorModel, data);
            if (retrievefilters == true)
            {
                JToken mappingFilters = JToken.FromObject(connectorDocument.filters);
                data[Constants.filters] = mappingFilters;
            }
            return data;
        }

        /// <summary>
        /// Gets all connections from cosmos
        /// </summary>
        /// <param name="filters">Filters to get different connections</param>
        /// <returns>List of Data Mapping Connections <see cref="DataMappingConnection"/></returns>
        public async Task<List<ConnectorModel>> GetConnectors(ConnectorFilterModel filters)
        {
            try
            {
                Expression<Func<ConnectorDocument, bool>> condition = null;

                if (filters?.EntityTypes?.Count > 0)
                {
                    var entityTypeFilter = filters.EntityTypes.Select(t => t.ToString());
                    condition = connector => entityTypeFilter.Contains(connector.info.entityType.ToString());
                }

                if (filters?.DataSources?.Count > 0)
                {
                    condition = connector => filters.DataSources.Contains(connector.info.dataSourceId);
                }

                if (filters?.RegistrationModes?.Count > 0)
                {
                    //TODO: Implement registration mode filtering here.
                }

                var connectors = await _cosmosService.GetConnectors(true, condition);

                return connectors.ToList();
            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<ConnectorModel> GetConnector(string connectorId)
        {
            var connector = await _cosmosService.GetConnector(connectorId, true);
            return connector;
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

                queryParameters = (from param in cdmQueryParameters
                                                  join requestParam in connectorModel.Request?.Parameters
                                                  on param.Key.ToString().ToLower() equals requestParam.CdmName.ToLower()
                                                  select new { name = requestParam.Name, value = param.Value.ToString() }).ToDictionary(c => c.name, c=> c.value);

                sortParameters = (from param in cdmQueryParameters
                                  join requestParam in connectorModel.Request?.Sorting?.Properties
                                  on param.Value.ToString().ToLower() equals requestParam.ElementValue.ToLower() 
                                  select new { name = requestParam.Element, value = requestParam.ElementValue.ToString() })
                                  .ToDictionary(c => c.name, c => c.value);
            }

            // Add default parameters if not present in the request. ex: locale, ResultSize etc
            var defaultParameters = connectorModel.Request?.Parameters.Where(t =>
                                 !String.IsNullOrWhiteSpace(t.DefaultValue) && !queryParameters.ContainsKey(t.Name))
                                .ToDictionary(c => c.Name, c => c.DefaultValue);

            var allParameters = defaultParameters.Concat(queryParameters).Concat(sortParameters);

            foreach (var parameter in allParameters)
            {
                modifiedQueryParameters.Add(parameter.Key, parameter.Value);
            }

            return modifiedQueryParameters;
        }

        private Dictionary<string, string> SetupPagination(ConnectorModel connectorModel, Dictionary<string, string> cdmQueryParameters)
        {
            // Ex: Office 365 uses 0 based offset numbering.
            if (connectorModel.Pagination?.IsZeroBasedOffset == true && cdmQueryParameters.ContainsKey("Offset"))
            {
                cdmQueryParameters["Offset"] = (int.Parse(cdmQueryParameters["Offset"]) - 1).ToString();
            }
            return cdmQueryParameters;
        }

        /// <summary>
        /// Enriches the connector data using the app's corresponding data source operations service
        /// </summary>
        /// <param name="connector">The data connector</param>
        /// <param name="data">The entity data</param>
        /// <returns></returns>
        private dynamic EnrichConnectorData(ConnectorModel connector, dynamic data)
        {
            var dataSourceOperationsService = _dataSourceOperationsFactory.GetDataSourceOperationsService(connector?.DataSource?.AppCode);
            if (dataSourceOperationsService != null)
                data = dataSourceOperationsService.SetItemLink(connector.Info.EntityType, data, connector.HostName);
            else
                _logger.LogInformation("No data source operations service found for {appCode}", connector?.DataSource?.AppCode ?? "");
            return data;
        }
    }
}
