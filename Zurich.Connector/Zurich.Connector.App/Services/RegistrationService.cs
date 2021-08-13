using System;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
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
        Task RegisterDataSource(string connectorId);
        /// <summary>
        /// Remove user from cosmosdb
        /// </summary>
        Task RemoveUserConnector(string connectorId);
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

        public async Task RegisterDataSource(string connectorId)
        {
            ConnectorRegistrationDocument cosmosDocument = new ConnectorRegistrationDocument()
            {
               
                ConnectorId = connectorId,
                partitionkey = _sessionAccesor.UserId.ToString(),
                UserId = _sessionAccesor.UserId,
                TenantId = _sessionAccesor.TenantId,
                Id = connectorId
            };
            await _cosmosService.StoreConnectorRegistration(cosmosDocument);
        }

        public async Task RemoveUserConnector(string connectorId)
        {
            await _cosmosService.RemoveConnectorRegistration(connectorId, _sessionAccesor.UserId.ToString());
        }

        public async Task GetUserConnections()
        {
            return _cosmosService.GetConnectorRegistrations(_sessionAccesor.UserId.ToString());
        }
    }
}
