using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common.Models.Connectors;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.TenantData;

namespace Zurich.Connector.App.Services
{
    public interface IRegistrationService
    {
        /// <summary>
        /// Registers a datasource to a given user
        /// </summary>
        /// <param name="connectorId">The connector id</param>
        /// <returns>Empty Task</returns>
        Task<bool> RegisterDataSource(string connectorId);
        /// <summary>
        /// Remove user from cosmosdb
        /// </summary>
        Task RemoveUserConnector(string connectorId);

        /// <summary>
        /// Gets list of user's registered connector ids
        /// </summary>
        /// <param name="registrationModes">List of registration modes to provide additional filtering</param>
        /// <returns>List of connector ids</returns>
        IEnumerable<string> GetUserConnections(IEnumerable<RegistrationEntityMode> registrationModes);
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly ICosmosService _cosmosService;
        private readonly ISessionAccessor _sessionAccesor;
        private readonly IConnectorService _connectorService;
        public RegistrationService(ICosmosService cosmosService, ISessionAccessor sessionAccesor, IConnectorService connectorService)
        {
            _cosmosService = cosmosService;
            _sessionAccesor = sessionAccesor;
            _connectorService = connectorService;
        }

        public async Task<bool> RegisterDataSource(string connectorId)
        {
            var connector = await _connectorService.GetConnector(connectorId);
            if(connector==null)
            {
                return false;
            }
            ConnectorRegistrationDocument cosmosDocument = new ConnectorRegistrationDocument()
            {

                ConnectorId = connectorId,
                partitionkey = _sessionAccesor.UserId.ToString(),
                UserId = _sessionAccesor.UserId.ToString(),
                TenantId = _sessionAccesor.TenantId.ToString(),
                Id = $"{_sessionAccesor.UserId}-{connectorId}"
            };
            await _cosmosService.StoreConnectorRegistration(cosmosDocument);
            return true;
        }

        public async Task RemoveUserConnector(string connectorId)
        {
            await _cosmosService.RemoveConnectorRegistration(connectorId, _sessionAccesor.UserId.ToString());
        }

        public IEnumerable<string> GetUserConnections(IEnumerable<RegistrationEntityMode> registrationModes)
        {
            var partitionKey = _sessionAccesor.UserId.ToString();

            // TODO: Add filtering by registration modes
            var connections = _cosmosService.GetConnectorRegistrations(partitionKey, null);
            var connectionIds = connections.Select(connection => connection.ConnectorId);

            return connectionIds;
        }
    }
}
