using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// <returns>Boolean indicating success</returns>
        Task<bool> RegisterDataSource(string connectorId);

        /// <summary>
        /// Remove user from cosmosdb
        /// </summary>
        /// <param name="connectorId">The connector id</param>
        /// <returns>Boolean indicating success</returns>
        Task<bool> RemoveUserConnector(string connectorId);

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

        public RegistrationService(ICosmosService cosmosService, ISessionAccessor sessionAccesor)
        {
            _cosmosService = cosmosService;
            _sessionAccesor = sessionAccesor;
        }

        public async Task<bool> RegisterDataSource(string connectorId)
        {
            if (string.IsNullOrEmpty(connectorId))
            {
                return false;
            }

            ConnectorRegistrationDocument cosmosDocument = new ConnectorRegistrationDocument()
            {
                ConnectorId = connectorId,
                partitionkey = _sessionAccesor.UserId.ToString(),
                UserId = _sessionAccesor.UserId,
                TenantId = _sessionAccesor.TenantId,
                Id = $"{_sessionAccesor.UserId}-{connectorId}"
            };

            await _cosmosService.StoreConnectorRegistration(cosmosDocument);
            return true;
        }

        public async Task<bool> RemoveUserConnector(string connectorId)
        {
            var userId = _sessionAccesor.UserId.ToString();

            Expression<Func<ConnectorRegistrationDocument, bool>> condition = registration => registration.ConnectorId == connectorId;
            var registrations = _cosmosService.GetConnectorRegistrations(userId, condition);
            if (registrations.Count() < 1)
            {
                return false;
            }

            foreach (var registration in registrations)
            {
                await _cosmosService.RemoveConnectorRegistration(registration.Id, userId);
            }
            return true;
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
