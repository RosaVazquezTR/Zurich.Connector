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
        /// <param name="dataSourceId">The data source id to register</param>
        /// <param name="connectorId">The connector id</param>
        /// <returns>Empty Task</returns>
        Task RegisterDataSource(string dataSourceId, string connectorId);
        /// <summary>
        /// Remove user from cosmosdb
        /// </summary>
        Task RemoveDataSource(string id);
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

        public async Task RegisterDataSource(string dataSourceId, string connectorId)
        {
            ConnectorRegistrationDocument cosmosDocument = new ConnectorRegistrationDocument()
            {
                DatasourceId = dataSourceId,
                ConnectorId = connectorId,
                partitionkey = _sessionAccesor.UserId.ToString(),
                UserId = _sessionAccesor.UserId,
                TenantId = _sessionAccesor.TenantId,
                Id = dataSourceId
            };
            await _cosmosService.StoreConnectorRegistration(cosmosDocument);
        }

        public async Task RemoveDataSource(string id)
        {
            await _cosmosService.DeleteConnectorAsync(id, _sessionAccesor.UserId.ToString());
        }
    }
}
