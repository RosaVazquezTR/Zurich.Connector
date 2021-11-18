using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Enum;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    /// <summary>
    /// Service to help facilitate connector operation
    /// </summary>
    public interface IConnectorService
    {
        /// <summary>
        /// Gets all connections from cosmos
        /// </summary>
        /// <param name="filters">Filters to get different connections</param>
        /// <returns>List of Data Mapping Connections <see cref="DataMappingConnection"/></returns>
        Task<List<ConnectorModel>> GetConnectors(Common.Models.Connectors.ConnectorFilterModel filters);

        Task<ConnectorModel> GetConnector(string connectorId);
    }

    public class ConnectorService : IConnectorService
    {
        private readonly ICosmosService _cosmosService;
        private readonly ILogger<ConnectorService> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly IConfiguration _configuration;

        public ConnectorService(
            ILogger<ConnectorService> logger,
            ICosmosService cosmosService,
            IRegistrationService registrationService,
            IConfiguration configuration)
        {
            _cosmosService = cosmosService;
            _logger = logger;
            _registrationService = registrationService;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets all connections from cosmos
        /// </summary>
        /// <param name="filters">Filters to get different connections</param>
        /// <returns>List of Data Mapping Connections <see cref="DataMappingConnection"/></returns>
        public async Task<List<ConnectorModel>> GetConnectors(Common.Models.Connectors.ConnectorFilterModel filters)
        {
            try
            {
                bool isEntityTypeFilter = false;
                bool isDataSourceFilter = false;
                IEnumerable<string> entityTypeFilter = Enumerable.Empty<string>();
                IEnumerable<string> dataSourceFilter = Enumerable.Empty<string>();
                IEnumerable<string> registeredDataSources = Enumerable.Empty<string>();
                if (filters?.EntityTypes?.Count > 0)
                {
                    isEntityTypeFilter = true;
                    entityTypeFilter = filters.EntityTypes.Select(t => t.ToString());
                }

                if (filters?.DataSources?.Count > 0)
                {
                    isDataSourceFilter = true;
                    dataSourceFilter = filters.DataSources;
                }

                var showPreReleaseConnectors = _configuration.GetValue<string>(AppSettings.ShowPreReleaseConnectors);
                bool blnShowPreReleaseConnectors;
                Boolean.TryParse(showPreReleaseConnectors, out blnShowPreReleaseConnectors);

                Expression<Func<ConnectorDocument, bool>> condition;

                if (blnShowPreReleaseConnectors)
                {
                    condition = connector => (connector.Info.SubType == SubType.Parent || !connector.Info.SubType.IsDefined())
                                 && (isEntityTypeFilter == false || entityTypeFilter.Contains(connector.Info.EntityType.ToString()))
                                 && (isDataSourceFilter == false || dataSourceFilter.Contains(connector.Info.DataSourceId));
                }
                else
                {
                    condition = connector => (!connector.PreRelease.IsDefined() || !connector.PreRelease)
                                 && (connector.Info.SubType == SubType.Parent || !connector.Info.SubType.IsDefined())
                                 && (isEntityTypeFilter == false || entityTypeFilter.Contains(connector.Info.EntityType.ToString()))
                                 && (isDataSourceFilter == false || dataSourceFilter.Contains(connector.Info.DataSourceId));
                }

                var connectors = await _cosmosService.GetConnectors(true, condition);

                if (filters.RegistrationModes != null && filters.RegistrationModes.Any())
                {
                    connectors = connectors.Where(x => filters.RegistrationModes.Contains(x.DataSource.RegistrationInfo.RegistrationMode));
                }

                registeredDataSources = await _registrationService.GetUserDataSources();
                List<ConnectorModel> registeredConnectors = new List<ConnectorModel>();
                foreach (var connector in connectors.Where(connector => registeredDataSources.Contains(connector.DataSource.AppCode)))
                {
                    connector.RegistrationStatus = RegistrationStatus.Registered;
                    registeredConnectors.Add(connector);
                }
                // Can't stick this in the cosmos query because it is looking at connectors not datasources.
                if(filters.IsRegistered)
                {
                    connectors = registeredConnectors;
                }

                return connectors.ToList();
            }
            catch (Exception ex)
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

    }
}
