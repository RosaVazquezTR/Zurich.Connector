using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Common.Models.Cosmos;
using Zurich.Connector.App.Model;
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
		private readonly IMapper _mapper;

		public CosmosService(ICosmosDocumentReader cosmosDocumentReader, 
							 ICosmosDocumentWriter cosmosDocumentWriter, 
							 CosmosClientSettings cosmosClientSettings,
							 IMapper mapper,
							 ILogger<CosmosService> logger)
		{
			_cosmosDocumentReader = cosmosDocumentReader;
			_cosmosDocumentWriter = cosmosDocumentWriter;
			_cosmosClientSettings = cosmosClientSettings;
			_logger = logger;
			_mapper = mapper;
		}

		/// <summary>
		/// Fetch connector from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public async Task<ConnectorModel> GetConnector(string connectorId)
        {
			var connectorDocument = await _cosmosDocumentReader.GetConnectorDocument(connectorId);
			return _mapper.Map<ConnectorModel>(connectorDocument);
		}

		/// <summary>
		/// Fetch all connectors from Cosmos
		/// </summary>
		/// <returns>Connector document list.</returns> 
		public async Task<IEnumerable<ConnectorModel>> GetConnectors(Expression<Func<ConnectorDocument, bool>> condition = null)
        {
            var connectorDocuments = await _cosmosDocumentReader.GetConnectorDocuments(condition);
			List<ConnectorModel> connectors = _mapper.Map<List<ConnectorModel>>(connectorDocuments.ToList());
			return connectors;
        }

		/// <summary>
		/// Fetch a data sources from Cosmos by dataSourceID
		/// </summary>
		/// <returns>data source.</returns> 
		public async Task<DataSourceModel> GetDataSource(string dataSourceId)
        {
			var dataSourceDocument = await _cosmosDocumentReader.GetDataSourceDocument(dataSourceId);
			return _mapper.Map<DataSourceModel>(dataSourceDocument);
		}

		/// <summary>
		/// Fetch all data sources from Cosmos
		/// </summary>
		/// <returns>List of data sources.</returns> 
		public async Task<IEnumerable<DataSourceModel>> GetDataSources(Expression<Func<DataSourceDocument, bool>> condition = null)
        {
			var dataSourceDocuments = await _cosmosDocumentReader.GetDataSourceDocuments(condition);
			List<DataSourceModel> dataSources = _mapper.Map<List<DataSourceModel>>(dataSourceDocuments.ToList());
			return dataSources;
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
