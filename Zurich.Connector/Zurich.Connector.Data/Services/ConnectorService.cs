﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;

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
        /// <returns>Mapped data for the connector</returns>
        Task<dynamic> GetConnectorData(string connectionId, string hostname, string transferToken);

        /// <summary>
        /// Gets all connections from the data mapping file
        /// </summary>
        /// <param name="filters">Filters to get different connections</param>
        /// <returns>List of Data Mapping Connections <see cref="DataMappingConnection"/></returns>
        Task<List<DataMappingConnection>> GetConnectors(ConnectorFilterModel filters);
    }

    public class ConnectorService : IConnectorService
    {
        private readonly IDataMapping _dataMapping;
        private readonly IDataMappingFactory _dataMappingFactory;
        private readonly IDataMappingRepository _dataMappingRepo;
        private readonly ILogger<ConnectorService> _logger;

        public ConnectorService(IDataMapping dataMapping, IDataMappingFactory dataMappingFactory, IDataMappingRepository dataMappingRepo, ILogger<ConnectorService> logger)
        {
            _dataMapping = dataMapping;
            _dataMappingFactory = dataMappingFactory;
            _dataMappingRepo = dataMappingRepo;
            _logger = logger;
        }

        public async Task<dynamic> GetConnectorData(string connectionId, string hostname, string transferToken)
        {
            DataMappingClass dataMappingInformation =  await _dataMapping.RetrieveProductInformationMap(connectionId, hostname);
            if (dataMappingInformation == null)
            {
                return null;
            }

            IDataMapping service = _dataMappingFactory.GetMapper(dataMappingInformation);

            return await service.Get<dynamic>(dataMappingInformation, transferToken);
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
    }
}
