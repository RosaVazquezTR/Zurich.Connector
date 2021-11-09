using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
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

        Task<ConnectorModel> GetConnector(string connectorId, string domain);
    }

    public class ConnectorService : IConnectorService
    {
        private readonly ICosmosService _cosmosService;
        private readonly ILogger<ConnectorService> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly OAuthOptions _oAuthOptions;

        public ConnectorService(
            ILogger<ConnectorService> logger,
            ICosmosService cosmosService,
            IRegistrationService registrationService,
            OAuthOptions oAuthOptions)
        {
            _cosmosService = cosmosService;
            _logger = logger;
            _registrationService = registrationService;
            _oAuthOptions = oAuthOptions;
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
                IEnumerable<string> registeredConnectors = Enumerable.Empty<string>();
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

                registeredConnectors = _registrationService.GetUserConnections(filters.RegistrationModes);

                Expression<Func<ConnectorDocument, bool>> condition = connector => (isEntityTypeFilter == false || entityTypeFilter.Contains(connector.Info.EntityType.ToString()))
                                        && (isDataSourceFilter == false || dataSourceFilter.Contains(connector.Info.DataSourceId))
                                        && (filters.IsRegistered == false || registeredConnectors.Contains(connector.Id));
                var connectors = await _cosmosService.GetConnectors(true, condition);

                if (filters.RegistrationModes != null && filters.RegistrationModes.Any())
                {
                    connectors = connectors.Where(x => filters.RegistrationModes.Contains(x.DataSource.RegistrationInfo.RegistrationMode));
                }
                foreach (var connector in connectors.Where(connector => registeredConnectors.Contains(connector.Id)))
                {
                    connector.RegistrationStatus = RegistrationStatus.Registered;
                }

                return connectors.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<ConnectorModel> GetConnector(string connectorId, string domain)
        {
            var connector = await _cosmosService.GetConnector(connectorId, true);

            var domainUrl = _oAuthOptions.Connections[connector.DataSource.AppCode].BaseUrl;
            if (connector.DataSource.RegistrationInfo.DomainRequired || (!string.IsNullOrEmpty(domain) && domain.Equals(domainUrl)))
            {
                return connector;
            }
            else
            {
                connector.DataSource.RegistrationInfo.DomainSpecificInformation = null;
            }
            return connector;
        }

    }
}
