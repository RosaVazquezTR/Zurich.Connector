using System;
using System.Threading.Tasks;
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
                UserId = _sessionAccesor.UserId,
                TenantId = _sessionAccesor.TenantId,
                Id = $"{_sessionAccesor.UserId}-{connectorId}"
            };
            await _cosmosService.StoreConnectorRegistration(cosmosDocument);
            return true;
        }

        public async Task RemoveUserConnector(string connectorId)
        {
            await _cosmosService.RemoveConnectorRegistration(connectorId, _sessionAccesor.UserId.ToString());
        }
       
    }
}
