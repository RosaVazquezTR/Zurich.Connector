﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
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
		public Task<IEnumerable<ConnectorModel>> GetConnectors(bool includeDataSource = false, Expression<Func<ConnectorDocument, bool>> condition = null);
		/// <summary>
		/// Fetch connector from Cosmos by ID
		/// </summary>
		/// <returns>Connector document.</returns> 
		public Task<ConnectorModel> GetConnector(string connectorId, bool includeDataSource = false);

		/// <summary>
		/// Fetch all data sources from Cosmos
		/// </summary>
		/// <returns>List of data sources.</returns> 
		public Task<IEnumerable<DataSourceModel>> GetDataSources(Expression<Func<DataSourceDocument, bool>> condition = null);

		/// <summary>
		/// Fetch a data sources from Cosmos by dataSourceID
		/// </summary>
		/// <returns>data source.</returns> 
		public Task<DataSourceModel> GetDataSource(string dataSourceId);

		/// <summary>
		/// Write connector document to cosmos
		/// </summary>
		public Task StoreConnector(ConnectorDocument connectorDocument);
		/// <summary>
		/// Write data source document to cosmos
		/// </summary>
		public Task StoreDataSource(DataSourceDocument dataSourceDocument);

		/// <summary>
		/// Store Connector Registration into the cosmosDb
		/// </summary>
		public Task StoreConnectorRegistration(ConnectorRegistrationDocument connectorRegistrationDocument);

		/// <summary>
		/// delete data  from Cosmos by ID
		/// </summary>
		public Task DeleteConnectorAsync(string connectorId, string partitionId);

	}
}
