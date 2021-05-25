using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Zurich.Common.Models.Cosmos;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Repositories
{
	/// <summary>
	/// Manages all cosmos read activities.
	/// </summary>
    public interface ICosmosDocumentReader
	{
		/// <summary>
		/// Fetche the list of connector document from Cosmos
		/// </summary>
		/// <returns>list of connector documents</returns>
		public Task<List<ConnectorDocument>> GetAllConnectorDocuments();

		/// <summary>
		/// Fetch connector document from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public Task<ConnectorDocument> GetConnectorDocument(string connectorId);

		/// <summary>
		/// Fetch connectors from Cosmos by a list of connectorID
		/// </summary>
		/// <returns>List of connector document.</returns> 
		public Task<List<ConnectorDocument>> GetConnectorDocuments(IEnumerable<string> connectorId);

		/// <summary>
		/// Fetche the list of data source document from Cosmos
		/// </summary>
		/// <returns>list of data source documents</returns>
		public Task<List<DataSourceDocument>> GetAllDataSourceDocuments();

		/// <summary>
		/// Fetch a data sources from Cosmos by dataSourceID
		/// </summary>
		/// <returns>data source.</returns> 
		public Task<DataSourceDocument> GetDataSourceDocument(string dataSourceId);
	}

	public class CosmosDocumentReader: ICosmosDocumentReader
	{
		CosmosDbOptions options;
		DocumentClient _documentClient;
		private readonly ILogger _logger;

		public CosmosDocumentReader(CosmosDbOptions cosmosOptions, ICosmosClientFactory cosmosClientFactory, ILogger<CosmosDocumentReader> logger) 
		{
			this.options = cosmosOptions;
			this._documentClient = cosmosClientFactory.DocumentClient;
			_logger = logger;
		}

		/// <summary>
		/// Fetche the list of connector document from Cosmos
		/// </summary>
		/// <returns>list of connector documents</returns>
		public async Task<List<ConnectorDocument>> GetAllConnectorDocuments()
		{
			try
			{
				var response = this._documentClient.CreateDocumentQuery<ConnectorDocument>
										(UriFactory.CreateDocumentCollectionUri(this.options.Database, CosmosConstants.ConnectorCollection))
										.AsEnumerable().ToList();

				return response;
			}
			catch(Exception ex)
            {
				_logger.LogError(ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Fetch connector document from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public async Task<ConnectorDocument> GetConnectorDocument(string connectorId)
        {
			try
			{
                var response = await this._documentClient.ReadDocumentAsync<ConnectorDocument>(
					UriFactory.CreateDocumentUri(this.options.Database, CosmosConstants.ConnectorCollection, connectorId),
                    new RequestOptions { PartitionKey = new PartitionKey(CosmosConstants.ConnectorList) });
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}	
		}

		/// <summary>
		/// Fetch connectors from Cosmos by a list of connectorID
		/// </summary>
		/// <returns>List of connector document.</returns> 
		public async Task<List<ConnectorDocument>> GetConnectorDocuments(IEnumerable<string> connectorId)
		{
			try
			{
				var response = this._documentClient.CreateDocumentQuery<ConnectorDocument>
										(UriFactory.CreateDocumentCollectionUri(this.options.Database, CosmosConstants.ConnectorCollection),
										new FeedOptions { EnableCrossPartitionQuery = true })
										.Where(t=> connectorId.Contains(t.Id) ).AsEnumerable().ToList();

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Fetche the list of data source document from Cosmos
		/// </summary>
		/// <returns>list of data source documents</returns>
		public async Task<List<DataSourceDocument>> GetAllDataSourceDocuments()
        {
			try
			{
				var response = this._documentClient.CreateDocumentQuery<DataSourceDocument>
										(UriFactory.CreateDocumentCollectionUri(this.options.Database, CosmosConstants.DataSourceCollection)).AsEnumerable().ToList();

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Fetch a data sources from Cosmos by dataSourceID
		/// </summary>
		/// <returns>data source.</returns> 
		public async Task<DataSourceDocument> GetDataSourceDocument(string dataSourceId)
        {
			try
			{
				var response = await this._documentClient.ReadDocumentAsync<DataSourceDocument>(
					UriFactory.CreateDocumentUri(this.options.Database, CosmosConstants.DataSourceCollection, dataSourceId),
					new RequestOptions { PartitionKey = new PartitionKey(CosmosConstants.DataSourceList) });
				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}
	}
}
