using AutoMapper;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Model.ConnectorModels;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.App.Services
{
    public class CosmosService: ICosmosService
	{
		private readonly ConnectorCosmosContext _cosmosContext;
		private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public CosmosService(ConnectorCosmosContext connectorCosmosContext, IMapper mapper, IConfiguration configuration)
		{
			_cosmosContext = connectorCosmosContext;
			_mapper = mapper;
            _configuration = configuration;
        }

        /// <summary>
        /// Fetch connector from Cosmos by ID
        /// </summary>
        /// <returns>Connector document.</returns> 
        public async Task<ConnectorModel> GetConnector(string connectorId, bool includeDataSource = false)
        {
            var connectorDocument = await _cosmosContext.GetDocument<ConnectorDocument>
                                        (CosmosConstants.ConnectorContainerId, connectorId, CosmosConstants.ConnectorPartitionKey);
            var connector = _mapper.Map<ConnectorModel>(connectorDocument);
            if (includeDataSource && connector != null)
            {
                connector.DataSource = await GetDataSource(connectorDocument.Info.DataSourceId);
            }
            return connector;
        }

        /// <summary>
        /// Fetches on prem instance data by ID
        /// </summary>
        /// <param name="connectorId">Id of the respective on prem instance</param>
        /// <returns>Instance model</returns>
        public async Task<OnPremInstanceModel> GetOnPremInstanceDetailsAsync(string connectorId)
        {
            var connectorDocument = await _cosmosContext.GetDocument<OnPremInstanceDocument>
                                        (CosmosConstants.OnPremContainerId, connectorId, CosmosConstants.OnPremPartitionKey);

            return _mapper.Map<OnPremInstanceModel>(connectorDocument);
        }

        /// <summary>
        /// Write connector document to cosmos
        /// </summary>
        public async Task StoreOnPremInstace(OnPremInstanceModel model)
        {
            var documentModel = _mapper.Map<OnPremInstanceDocument>(model);
            await _cosmosContext.UpsertDocument(documentModel, CosmosConstants.OnPremContainerId);
        }

        /// <summary>
        /// Get connector from Cosmos by Alias
        /// </summary>
        /// <returns>Connector document.</returns> 
        public async Task<ConnectorModel> GetConnectorByAlias(string alias, bool includeDataSource = false)
        {
            var connectorDocuments = _cosmosContext.GetDocuments<ConnectorDocument>(
                    CosmosConstants.ConnectorContainerId,
                    CosmosConstants.ConnectorPartitionKey,
                    null
                );
            var connectorDocument = connectorDocuments?.Where(connectorDocument => !string.IsNullOrEmpty(connectorDocument.Alias) && connectorDocument.Alias.Equals(alias, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            var connector = _mapper.Map<ConnectorModel>(connectorDocument);

            if (includeDataSource && connector != null)
            {
                connector.DataSource = await GetDataSource(connectorDocument.Info.DataSourceId);
            }

            return connector;
        }

        /// <summary>
        /// Fetch all connectors from Cosmos
        /// </summary>
        /// <returns>Connector document list.</returns> 
        public async Task<IEnumerable<ConnectorModel>> GetConnectors(bool includeDataSource = false, Expression<Func<ConnectorDocument, bool>> condition = null)
        {
            var connectorDocuments = _cosmosContext.GetDocuments(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, condition);
            var connectors = _mapper.Map<IEnumerable<ConnectorModel>>(connectorDocuments);

            if (includeDataSource && connectors != null)
            {
                var dataSourceIDs = connectors.Select(t => t.Info.DataSourceId).Distinct();
                var dataSourceResult = await GetDataSources(dataSources => dataSourceIDs.Contains(dataSources.Id));

                foreach (var connector in connectors)
                {
                    connector.DataSource = dataSourceResult.FirstOrDefault(d => d.Id == connector.Info.DataSourceId);
                }
            }

            return connectors;
        }

        /// <summary>
        /// Fetch all connectors from Cosmos
        /// </summary>
        /// <returns>Connector document list.</returns> 
        public async Task<ConnectorModel> GetConnectorUsingPreRelease(string connectorId, bool includeDataSource = false)
        {
            var showPreReleaseConnectors = _configuration.GetValue<string>(AppSettings.ShowPreReleaseConnectors);
            bool blnShowPreReleaseConnectors;
            Boolean.TryParse(showPreReleaseConnectors, out blnShowPreReleaseConnectors);
            Expression<Func<ConnectorDocument, bool>> condition;
            condition = connector => (connector.Id == connectorId && (blnShowPreReleaseConnectors || (!connector.PreRelease.IsDefined() || !connector.PreRelease)));

            var connectorDocuments = _cosmosContext.GetDocuments(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, condition);
            var connectors = _mapper.Map<List<ConnectorModel>>(connectorDocuments);
            var connector = connectors.SingleOrDefault();

            if (includeDataSource && connectors.Count > 0)
            {
                var dataSourceIDs = connectors.Select(t => t.Info.DataSourceId).Distinct();

                Expression<Func<DataSourceDocument, bool>> dsCondition = dataSources => dataSourceIDs.Contains(dataSources.Id);
                var dataSourceResult = await GetDataSources(dsCondition);
                connector.DataSource = dataSourceResult.Where(d => d.Id == connector.Info.DataSourceId).FirstOrDefault();
               
            }

            return connector;
        }

        /// <summary>
        /// Fetch a data sources from Cosmos by dataSourceID
        /// </summary>
        /// <returns>data source.</returns> 
        public async Task<DataSourceModel> GetDataSource(string dataSourceId)
        {
            var dataSourceDocument = await _cosmosContext.GetDocument<DataSourceDocument>
                                        (CosmosConstants.DataSourceContainerId, dataSourceId, CosmosConstants.DataSourcePartitionKey);
            return _mapper.Map<DataSourceModel>(dataSourceDocument);
        }

        /// <summary>
        /// Fetch all data sources from Cosmos
        /// </summary>
        /// <returns>List of data sources.</returns> 
        public async Task<IEnumerable<DataSourceModel>> GetDataSources(Expression<Func<DataSourceDocument, bool>> condition = null)
        {
            var dataSourceDocuments = _cosmosContext.GetDocuments(CosmosConstants.DataSourceContainerId, CosmosConstants.DataSourcePartitionKey, condition);
            return _mapper.Map<IEnumerable<DataSourceModel>>(dataSourceDocuments);
        }

        /// <summary>
        /// Write connector document to cosmos
        /// </summary>
        public async Task StoreConnector(ConnectorDocument connectorDocument)
        {
            await _cosmosContext.UpsertDocument(connectorDocument, CosmosConstants.ConnectorContainerId);
        }

        /// <summary>
        /// Write data source document to cosmos
        /// </summary>
        public async Task StoreDataSource(DataSourceDocument dataSourceDocument)
        {
            await _cosmosContext.UpsertDocument(dataSourceDocument, CosmosConstants.DataSourceContainerId);
        }

        /// <summary>
        /// Write connector registration document to cosmos
        /// </summary>
        public async Task StoreConnectorRegistration(ConnectorRegistrationDocument connectorRegistrationDocument)
        {
            await _cosmosContext.UpsertDocument(connectorRegistrationDocument, CosmosConstants.ConnectorRegistrationContainerId);
        }

        /// <summary>
        /// Get connector from CosmosDb by Id
        /// </summary>
        public async Task<ConnectorRegistration> GetUserRegistration(string connectorId, string UserId)
        {
            var userRegistration = await _cosmosContext.GetDocument<ConnectorRegistrationDocument>
                                        (CosmosConstants.ConnectorRegistrationContainerId, connectorId, UserId);
            return _mapper.Map<ConnectorRegistration>(userRegistration);
        }

        /// <summary>
		/// Get a list of Connector Registration Documents by user id
		/// </summary>
		public IEnumerable<ConnectorRegistration> GetConnectorRegistrations(string userId, Expression<Func<ConnectorRegistrationDocument, bool>> condition = null)
        {
            var registeredConnectorDocuments = _cosmosContext.GetDocuments(CosmosConstants.ConnectorRegistrationContainerId, userId, condition);
            List<ConnectorRegistration> connectorRegistrations = _mapper.Map<List<ConnectorRegistration>>(registeredConnectorDocuments);

            return connectorRegistrations;
        }

        /// <summary>
        /// delete data  from Cosmos by ID
        /// </summary>
        public async Task RemoveConnectorRegistration(string connectorId, string userId)
        {
            await _cosmosContext.DeleteDocument<ConnectorRegistrationDocument>(CosmosConstants.ConnectorRegistrationContainerId, connectorId, userId);
        }
    }
}