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
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.App.Services;
using System.Collections.Specialized;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

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
        /// Gets all connections from the data mapping file
        /// </summary>
        /// <param name="filters">Filters to get different connections</param>
        /// <returns>List of Data Mapping Connections <see cref="DataMappingConnection"/></returns>
        Task<List<DataMappingConnection>> GetConnectors(ConnectorFilterModel filters);
        Task<ConnectorsConfigResponseEntity> GetConnectorConfiguration(string connectorId);
    }

    public class ConnectorService : IConnectorService
    {
        private readonly IDataMapping _dataMapping;
        private readonly IDataMappingFactory _dataMappingFactory;
        private readonly IDataMappingRepository _dataMappingRepo;
        private readonly ILogger<ConnectorService> _logger;
        private readonly IMapper _mapper;
        private readonly ICosmosService _cosmosService;

        public ConnectorService(IDataMapping dataMapping, IDataMappingFactory dataMappingFactory, IDataMappingRepository dataMappingRepo, ILogger<ConnectorService> logger, IMapper mapper, ICosmosService CosmosService)
        {
            _dataMapping = dataMapping;
            _dataMappingFactory = dataMappingFactory;
            _dataMappingRepo = dataMappingRepo;
            _logger = logger;
            _mapper = mapper;
            _cosmosService = CosmosService;
        }

        public async Task<dynamic> GetConnectorData(string connectionId, string hostname, string transferToken, Dictionary<string, string> queryParameters)
        {
            NameValueCollection mappedQueryParameters;
            mappedQueryParameters = await MapQueryParametersFromDB(connectionId, queryParameters);

            DataMappingClass dataMappingInformation =  await _dataMapping.RetrieveProductInformationMap(connectionId, hostname);
            if (dataMappingInformation == null)
            {
                return null;
            }

            IDataMapping service = _dataMappingFactory.GetMapper(dataMappingInformation);

            return await service.Get<dynamic>(dataMappingInformation, transferToken, mappedQueryParameters);
        }

        public async Task<List<DataMappingConnection>> GetConnectors(ConnectorFilterModel filters)
        {
            IEnumerable<DataMappingConnection> connectors = await _dataMappingRepo.GetConnectors();

            // TODO: This filtering will eventually move to the repo on the DB call
            if (filters.EntityTypes?.Count > 0)
            {
                connectors = connectors.Where(connector => filters.EntityTypes.Contains(connector.EntityType));
            }

            if (filters.DataSources?.Count > 0)
            {
                connectors = connectors.Where(connector => filters.DataSources.Contains(connector.AppCode));
            }

            if (filters.AuthTypes?.Count > 0)
            {
                connectors = connectors.Where(connector => filters.AuthTypes.Contains(connector.Auth.Type));
            }

            return connectors.ToList();
        }

        public async Task<ConnectorsConfigResponseEntity> GetConnectorConfiguration(string ConnectorId)
        {
            var connectorconfiguration = await this._dataMappingRepo.GetConnectorConfiguration(ConnectorId);
            return this._mapper.Map<ConnectorsConfigResponseEntity>(connectorconfiguration);
        }

        private async Task<NameValueCollection>  MapQueryParametersFromDB(string id, Dictionary<string, string> queryParameters)
        {
            NameValueCollection modifiedQueryParameters = new NameValueCollection();

            var connectorDocument = await _cosmosService.GetConnector(id);
            
            var dataForNameValueCollection = (from param in queryParameters
                                              join requestParam in connectorDocument.request.parameters
                                              on param.Key.ToString().ToLower() equals requestParam.cdmName.ToLower()
                                              select new { name = requestParam.name, value = param.Value.ToString() }).ToList();

            dataForNameValueCollection.ForEach(data => modifiedQueryParameters.Add(data.name, data.value));
            modifiedQueryParameters = dataForNameValueCollection.Count>0 ? modifiedQueryParameters : null;
            
            return modifiedQueryParameters;

        }
    }
}
