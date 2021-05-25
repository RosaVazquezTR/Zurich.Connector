using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Common.Models.Cosmos;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.App.Services
{
    public class CosmosService: ICosmosService
	{
		private readonly ICosmosDocumentReader _cosmosDocumentReader;
		private readonly ICosmosDocumentWriter _cosmosDocumentWriter;
		private readonly CosmosClientSettings _cosmosClientSettings;
		private readonly ILogger _logger;
		public CosmosService(ICosmosDocumentReader cosmosDocumentReader, ICosmosDocumentWriter cosmosDocumentWriter, CosmosClientSettings cosmosClientSettings, ILogger<CosmosService> logger)
		{
			_cosmosDocumentReader = cosmosDocumentReader;
			_cosmosDocumentWriter = cosmosDocumentWriter;
			_cosmosClientSettings = cosmosClientSettings;
			_logger = logger;
		}

		/// <summary>
		/// Fetch connector from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public async Task<ConnectorDocument> GetConnector(string connectorId)
        {
			var connectorDocument = await _cosmosDocumentReader.GetConnectorDocument(connectorId);
			return connectorDocument;
		}

		/// <summary>
		/// Fetch all connectors from Cosmos
		/// </summary>
		/// <returns>Connector document list.</returns> 
		public async Task<List<ConnectorDocument>> GetAllConnectors()
        {
            var connectorDocuments = await _cosmosDocumentReader.GetAllConnectorDocuments();
            return connectorDocuments;
        }

		/// <summary>
		/// Fetch connectors from Cosmos by a list of connectorID
		/// </summary>
		/// <returns>List of connector document.</returns> 
		public async Task<List<ConnectorDocument>> GetConnectors(IEnumerable<string> connectorId)
		{
			var connectorDocuments = await _cosmosDocumentReader.GetConnectorDocuments(connectorId);
			return connectorDocuments;
		}

		/// <summary>
		/// Fetch a data sources from Cosmos by dataSourceID
		/// </summary>
		/// <returns>data source.</returns> 
		public async Task<DataSourceDocument> GetDataSource(string dataSourceId)
        {
			var dataSourceDocument = await _cosmosDocumentReader.GetDataSourceDocument(dataSourceId);
			return dataSourceDocument;
		}

		/// <summary>
		/// Fetch all data sources from Cosmos
		/// </summary>
		/// <returns>List of data sources.</returns> 
		public async Task<List<DataSourceDocument>> GetAllDataSources()
        {
			var dataSourceDocuments = await _cosmosDocumentReader.GetAllDataSourceDocuments();
			return dataSourceDocuments;
		}

		/// <summary>
		/// Write connector document to cosmos
		/// </summary>
		public async Task StoreConnector(ConnectorDocument connectorDocument)
        {
			await _cosmosDocumentWriter.StoreConnector(connectorDocument);

		}

		/// <summary>
		/// Write data source document to cosmos
		/// </summary>
		public async Task StoreDataSource(DataSourceDocument dataSourceDocument)
		{
			await _cosmosDocumentWriter.StoreDataSoruce(dataSourceDocument);

		}
	}
}
