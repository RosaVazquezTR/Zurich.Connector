using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.Cosmos;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Repositories
{
    /// <summary>
    /// Interface to manage all write activities to Cosmos.
    /// </summary>
    public interface ICosmosDocumentWriter
    {
        /// <summary>
		/// Write connector document to cosmos
		/// </summary>
        public Task StoreConnector(ConnectorDocument connectorDocument);

        /// <summary>
		/// Write data source document to cosmos
		/// </summary>
        public Task StoreDataSoruce(DataSourceDocument dataSourceDocument);
    }
    public class CosmosDocumentWriter : ICosmosDocumentWriter
    {
        CosmosDbOptions _cosmosOptions;
        DocumentClient _documentClient;
        CosmosClient _cosmosClient;
        ILogger<CosmosDocumentWriter> _log;

        public CosmosDocumentWriter(CosmosDbOptions cosmosOptions, ICosmosClientFactory cosmosClientFactory, ILogger<CosmosDocumentWriter> logger)
        {
            this._cosmosOptions = cosmosOptions;
            this._documentClient = cosmosClientFactory.DocumentClient;
            this._cosmosClient = cosmosClientFactory.CosmosClient;
            this._log = logger;
        }

        /// <summary>
		/// Write connector document to cosmos
		/// </summary>
        public async Task StoreConnector(ConnectorDocument connectorDocument)
        {
            await this._documentClient.UpsertDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(this._cosmosOptions.Database, CosmosConstants.ConnectorCollection),
                connectorDocument);
        }

        /// <summary>
		/// Write data source document to cosmos
		/// </summary>
        public async Task StoreDataSoruce(DataSourceDocument dataSourceDocument)
        {
            await this._documentClient.UpsertDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(this._cosmosOptions.Database, CosmosConstants.DataSourceCollection),
                dataSourceDocument);
        }
    }
}
