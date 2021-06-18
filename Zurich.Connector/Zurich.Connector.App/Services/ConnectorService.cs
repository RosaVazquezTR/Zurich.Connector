using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
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
using Zurich.Connector.App.Model;

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
        Task<dynamic> GetConnectorData(string connectionId, string hostname, string transferToken, Dictionary<string, string> queryParameters);

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

        public ConnectorService(IDataMapping dataMapping, IDataMappingFactory dataMappingFactory, IDataMappingRepository dataMappingRepo, 
                                ILogger<ConnectorService> logger, IMapper mapper, ICosmosService cosmosService)
        {
            _dataMapping = dataMapping;
            _dataMappingFactory = dataMappingFactory;
            _dataMappingRepo = dataMappingRepo;
            _cosmosService = cosmosService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<dynamic> GetConnectorData(string connectionId, string hostname, string transferToken, Dictionary<string, string> queryParameters)
        {
            NameValueCollection mappedQueryParameters;
            mappedQueryParameters = await MapQueryParametersFromDB(connectionId, queryParameters);

            var dataMappingInformation =  await _dataMapping.RetrieveProductInformationMap(connectionId, hostname);
            if (dataMappingInformation == null)
            {
                return null;
            }

            IDataMapping service = _dataMappingFactory.GetMapper(dataMappingInformation);

            return await service.Get<dynamic>(dataMappingInformation, transferToken, mappedQueryParameters);
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

        private async Task<NameValueCollection>  MapQueryParametersFromDB(string id, Dictionary<string, string> queryParameters)
        {
            NameValueCollection modifiedQueryParameters = new NameValueCollection();

            if (queryParameters.Any())
            {
                var connectorDocument = await _cosmosService.GetConnector(id);
            
                var dataForNameValueCollection = (from param in queryParameters
                                                  join requestParam in connectorDocument.Request?.Parameters
                                                  on param.Key.ToString().ToLower() equals requestParam.CdmName.ToLower()
                                                  select new { name = requestParam.Name, value = param.Value.ToString() }).ToList();

                dataForNameValueCollection.ForEach(data => modifiedQueryParameters.Add(data.name, data.value));
                modifiedQueryParameters = dataForNameValueCollection.Count>0 ? modifiedQueryParameters : null;
            }

            return modifiedQueryParameters;
        }
    }
}
