using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Common.Repositories.Cosmos;
using Zurich.Common.Models.Cosmos;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.TenantData;

namespace Zurich.Connector.App.Services
{
    public class CosmosService: ICosmosService
	{
		private readonly ICosmosClientStore _cosmosClientStore;
		private readonly CosmosClientSettings _cosmosClientSettings;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;

		public CosmosService(ICosmosClientStore cosmosClientStore,
							 CosmosClientSettings cosmosClientSettings,
							 IMapper mapper,
							 ILogger<CosmosService> logger)
		{
			_cosmosClientStore = cosmosClientStore;
			_cosmosClientSettings = cosmosClientSettings;
			_logger = logger;
			_mapper = mapper;
		}

		/// <summary>
		/// Fetch connector from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public async Task<ConnectorModel> GetConnector(string connectorId, bool includeDataSource = false)
        {
			var connectorDocument = await _cosmosClientStore.GetDocument<ConnectorDocument>
										(CosmosConstants.ConnectorContainerId, connectorId, CosmosConstants.ConnectorPartitionKey);
			 var connector = _mapper.Map<ConnectorModel>(connectorDocument);
            if (includeDataSource && connector != null)
            {
                connector.DataSource = await GetDataSource(connectorDocument.info.dataSourceId);
            }
            return connector;
		}

		/// <summary>
		/// Fetch all connectors from Cosmos
		/// </summary>
		/// <returns>Connector document list.</returns> 
		public async Task<IEnumerable<ConnectorModel>> GetConnectors(bool includeDataSource = false, Expression<Func<ConnectorDocument, bool>> condition = null)
        {
			var connectorDocuments = _cosmosClientStore.GetDocuments(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, condition);
			var connectors = _mapper.Map<List<ConnectorModel>>(connectorDocuments);

            if (includeDataSource && connectors != null)
            {
                var dataSourceIDs = connectors.Select(t => t.Info.DataSourceId).Distinct();

                Expression<Func<DataSourceDocument, bool>> dsCondition = dataSources => dataSourceIDs.Contains(dataSources.Id);
				var dataSourceResult = await GetDataSources(dsCondition);
                foreach (var connector in connectors)
                {
                    connector.DataSource = dataSourceResult.Where(d => d.Id == connector.Info.DataSourceId).FirstOrDefault();
                }
            }

            return connectors;
        }

		/// <summary>
		/// Fetch a data sources from Cosmos by dataSourceID
		/// </summary>
		/// <returns>data source.</returns> 
		public async Task<DataSourceModel> GetDataSource(string dataSourceId)
        {
			var dataSourceDocument = await _cosmosClientStore.GetDocument<DataSourceDocument>
										(CosmosConstants.DataSourceContainerId, dataSourceId, CosmosConstants.DataSourcePartitionKey);
			return _mapper.Map<DataSourceModel>(dataSourceDocument);
        }

		/// <summary>
		/// Fetch all data sources from Cosmos
		/// </summary>
		/// <returns>List of data sources.</returns> 
		public async Task<IEnumerable<DataSourceModel>> GetDataSources(Expression<Func<DataSourceDocument, bool>> condition = null)
        {
            var dataSourceDocuments = _cosmosClientStore.GetDocuments(CosmosConstants.DataSourceContainerId, CosmosConstants.DataSourcePartitionKey, condition);
            List<DataSourceModel> dataSources = _mapper.Map<List<DataSourceModel>>(dataSourceDocuments);
            return dataSources;
        }

		/// <summary>
		/// Write connector document to cosmos
		/// </summary>
		public async Task StoreConnector(ConnectorDocument connectorDocument)
        {
			await _cosmosClientStore.UpsertDocument(connectorDocument, CosmosConstants.ConnectorContainerId);
		}

		/// <summary>
		/// Write data source document to cosmos
		/// </summary>
		public async Task StoreDataSource(DataSourceDocument dataSourceDocument)
		{
			await _cosmosClientStore.UpsertDocument(dataSourceDocument, CosmosConstants.DataSourceContainerId);
		}

		/// <summary>
		/// Write connector registration document to cosmos
		/// </summary>
		public async Task StoreConnectorRegistration(ConnectorRegistrationDocument connectorRegistrationDocument)
        {
			
			await _cosmosClientStore.UpsertDocument(connectorRegistrationDocument, CosmosConstants.ConnectorRegistrationContainerId);
		  
		}
		/// <summary>
		/// delete data  from Cosmos by ID
		/// </summary>
		public async Task DeleteConnectorAsync(string connectorId, string partitionId)
		{
			await _cosmosClientStore.DeleteDocument<ConnectorRegistrationDocument>(CosmosConstants.ConnectorRegistrationContainerId, connectorId, partitionId);
		}

	}
}