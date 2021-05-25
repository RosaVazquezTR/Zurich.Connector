using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.App.Services
{
	/// <summary>
	/// Service to manage cosmos read and write operations.
	/// </summary>
	public interface ICosmosService
	{
		/// <summary>
		/// Fetch all connectors from Cosmos
		/// </summary>
		/// <returns>Connector document list.</returns> 
		public Task<List<ConnectorDocument>> GetAllConnectors();

		/// <summary>
		/// Fetch connector from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public Task<ConnectorDocument> GetConnector(string connectorId);

		/// <summary>
		/// Fetch connectors from Cosmos by a list of connectorID
		/// </summary>
		/// <returns>List of connector document.</returns> 
		public Task<List<ConnectorDocument>> GetConnectors(IEnumerable<string> connectorId);

		/// <summary>
		/// Fetch all data sources from Cosmos
		/// </summary>
		/// <returns>List of data sources.</returns> 
		public Task<List<DataSourceDocument>> GetAllDataSources();

		/// <summary>
		/// Fetch a data sources from Cosmos by dataSourceID
		/// </summary>
		/// <returns>data source.</returns> 
		public Task<DataSourceDocument> GetDataSource(string dataSourceId);

		/// <summary>
		/// Write connector document to cosmos
		/// </summary>
		public Task StoreConnector(ConnectorDocument connectorDocument);
		/// <summary>
		/// Write data source document to cosmos
		/// </summary>
		public Task StoreDataSource(DataSourceDocument dataSourceDocument);
	}
}
