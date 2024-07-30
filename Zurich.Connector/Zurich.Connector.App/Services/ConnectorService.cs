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
using Zurich.Connector.Web.Models;

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
        Task<IEnumerable<ConnectorModel>> GetConnectors(ConnectorFilterModel filters);

        /// <summary>
        /// Gets the data of an especific conector by its id or alias
        /// </summary>
        /// <param name="connectorId"> The alias or id of a given connector </param>
        /// <returns> Connector details </returns>
        Task<ConnectorModel> GetConnector(string connectorId);

        Task<bool> RevokeTenantApplication(string connectorId);
    }

    public class ConnectorService : IConnectorService
    {
        private readonly ICosmosService _cosmosService;
        private readonly ILogger<ConnectorService> _logger;
        private readonly IRegistrationService _registrationService;
        private readonly IConfiguration _configuration;
        private readonly IOAuthServices _OAuthService;

        public ConnectorService(
            ILogger<ConnectorService> logger,
            ICosmosService cosmosService,
            IRegistrationService registrationService,
            IConfiguration configuration,
            IOAuthServices OAuthService)
        {
            _cosmosService = cosmosService;
            _logger = logger;
            _registrationService = registrationService;
            _configuration = configuration;
            _OAuthService = OAuthService;
        }

        /// <summary>
        /// Gets all connections from cosmos
        /// </summary>
        /// <param name="filters">Filters to get different connections</param>
        /// <returns>List of Data Mapping Connections <see cref="DataMappingConnection"/></returns>
        public async Task<IEnumerable<ConnectorModel>> GetConnectors(ConnectorFilterModel filters)
        {
            try
            {
                bool isEntityTypeFilter = false;
                bool isDataSourceFilter = false;
                IEnumerable<string> entityTypeFilter = Enumerable.Empty<string>();
                IEnumerable<string> dataSourceFilter = Enumerable.Empty<string>();
                IEnumerable<DataSourceInformation> registeredDataSources = Enumerable.Empty<DataSourceInformation>();

                if (filters?.EntityTypes?.Any() ?? false)
                {
                    isEntityTypeFilter = true;
                    entityTypeFilter = filters.EntityTypes.Select(t => t.ToString());
                }

                if (filters?.DataSources?.Any() ?? false)
                {
                    isDataSourceFilter = true;
                    dataSourceFilter = filters.DataSources;
                }

                var showPreReleaseConnectors = _configuration.GetValue<string>(AppSettings.ShowPreReleaseConnectors);
                bool blnShowPreReleaseConnectors = Convert.ToBoolean(showPreReleaseConnectors);

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

                if (filters.RegistrationMode != null && filters.RegistrationMode.Any())
                {
                    connectors = connectors.Where(x => filters.RegistrationMode.Contains(x.DataSource.RegistrationInfo.RegistrationMode));
                }

                registeredDataSources = await _registrationService.GetUserDataSources();
                List<ConnectorModel> registeredConnectors = new List<ConnectorModel>();

                // If the user is new and does not have any connectors registered, this validation will avoid getting a 500 response
                if (registeredDataSources != null)
                {
                    foreach (var connector in connectors.Where(connector => registeredDataSources.Select(x => x.AppCode).Contains(connector.DataSource.AppCode)))
                    {
                        connector.RegistrationStatus = RegistrationStatus.Registered;
                        // FirstOrDefault is needed because we can get multiple app codes in the registered data sources. Eg: HighQ returns 2 app codes if a user is associated with 2 instances.
                        connector.DataSource.RequiresNewToken = registeredDataSources.Where(x => x.AppCode == connector.DataSource.AppCode).FirstOrDefault().RequiresNewToken;
                        registeredConnectors.Add(connector);
                    }
                }
                // Can't stick this in the cosmos query because it is looking at connectors not datasources.
                if (filters.IsRegistered)
                {
                    connectors = registeredConnectors;
                }

                return connectors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<ConnectorModel> GetConnector(string connectorId)
        {
            ConnectorModel connector;
            IEnumerable<DataSourceInformation> registeredDataSources = Enumerable.Empty<DataSourceInformation>();
            // Adding a validation to know wheter the call is being made providing connector id or connector alias
            if (int.TryParse(connectorId, out var _))
            {
                // This means we passed connector id
                connector = await _cosmosService.GetConnectorUsingPreRelease(connectorId, true);
            }
            else
            {
                // This means we passed connector alias
                connector = await _cosmosService.GetConnectorByAlias(connectorId, true);
            }
            if (connector != null)
            {
                try
                {
                    registeredDataSources = await _registrationService.GetUserDataSources();
                    if (registeredDataSources != null)
                    {
                        var registeredConnectors = from regCon in registeredDataSources where regCon.AppCode == connector.DataSource.AppCode select regCon;
                        if (registeredConnectors != null && registeredConnectors.Count() != 0)
                        {
                            connector.RegistrationStatus = RegistrationStatus.Registered;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error trying to change registered status, this will not be updated", ex.Message);
                }
            }

            return connector;
        }

        public async Task<bool> RevokeTenantApplication(string connectorId)
        {
            var connectorDetails = await GetConnector(connectorId);
            var result = await _OAuthService.RevokeTenantApplication(connectorDetails.DataSource.AppCode);
            return result;
        }

    }
}
