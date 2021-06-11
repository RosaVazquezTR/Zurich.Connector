using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

		public Task<IEnumerable<ConnectorDocument>> GetConnectorDocuments(Expression<Func<ConnectorDocument, bool>> condition = null);

		/// <summary>
		/// Fetch connector document from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public Task<ConnectorDocument> GetConnectorDocument(string connectorId);

		/// <summary>
		/// Fetche the list of data source document from Cosmos
		/// </summary>
		/// <returns>list of data source documents</returns>
		public Task<IEnumerable<DataSourceDocument>> GetDataSourceDocuments(Expression<Func<DataSourceDocument, bool>> condition = null);

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
		public async Task<IEnumerable<ConnectorDocument>> GetConnectorDocuments(Expression<Func<ConnectorDocument, bool>> condition = null)
		{
			try
			{
				IQueryable<ConnectorDocument> response = null;

				response = _documentClient.CreateDocumentQuery<ConnectorDocument>(UriFactory.CreateDocumentCollectionUri(this.options.Database, CosmosConstants.ConnectorCollection), new FeedOptions { PartitionKey = new PartitionKey(CosmosConstants.ConnectorList) });
				if (condition != null)
				{
					response = response.Where(condition);
				}
				return response.AsEnumerable();
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
		/// Fetche the list of data source document from Cosmos
		/// </summary>
		/// <returns>list of data source documents</returns>
		public async Task<IEnumerable<DataSourceDocument>> GetDataSourceDocuments(Expression<Func<DataSourceDocument, bool>> condition = null)
        {
			try
			{
				IQueryable<DataSourceDocument> response = null;

				response = _documentClient.CreateDocumentQuery<DataSourceDocument>(UriFactory.CreateDocumentCollectionUri(this.options.Database, CosmosConstants.DataSourceCollection), new FeedOptions { PartitionKey = new PartitionKey(CosmosConstants.DataSourceList) });
				if (condition != null)
				{
					response = response.Where(condition);
				}
				return response.AsEnumerable();
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
