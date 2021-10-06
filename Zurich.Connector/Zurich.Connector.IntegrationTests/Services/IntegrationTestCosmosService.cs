using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.App.Services
{
    public class IntegrationTestCosmosService: ICosmosService
	{
		private readonly ConnectorCosmosContext _cosmosContext;
		private readonly IMapper _mapper;
        private readonly List<ConnectorDocument> _connectors;
        private readonly List<DataSourceDocument> _dataSources;
        private const string folderLocation = @"..\..\..\..\..\Zurich.Connector.Deploy\CosmosRecords";

        public IntegrationTestCosmosService(ConnectorCosmosContext connectorCosmosContext, IMapper mapper)
		{
			_cosmosContext = connectorCosmosContext;
			_mapper = mapper;
            _connectors = MakeCosmosRecordsFromJson<ConnectorDocument>("\\connector");
            _dataSources = MakeCosmosRecordsFromJson<DataSourceDocument>("\\datasource");
        }

        private List<T> MakeCosmosRecordsFromJson<T>(string path)
        {
            List<T> comosRecords = new List<T>();
            string[] fileEntries = Directory.GetFiles($"{folderLocation}{path}");

            foreach (var fileLocation in fileEntries)
            {
                T currentRecord = default(T);
                using (StreamReader r = new StreamReader(fileLocation))
                {
                    string json = r.ReadToEnd();
                    currentRecord = JsonConvert.DeserializeObject<T>(json);
                }
                if (currentRecord != null)
                {
                    comosRecords.Add(currentRecord);
                }
            }
            return comosRecords;
        }

        /// <summary>
        /// Fetch connector from Cosmos by ID
        /// </summary>
        /// <returns>Connector document.</returns> 
        public async Task<ConnectorModel> GetConnector(string connectorId, bool includeDataSource = false)
        {
            var connectorDocument = _connectors.FirstOrDefault(x => x.Id == connectorId);
            var connector = _mapper.Map<ConnectorModel>(connectorDocument);
            if (includeDataSource && connector != null)
            {
                connector.DataSource = await GetDataSource(connectorDocument.Info.DataSourceId);
            }
            return connector;
        }

        /// <summary>
        /// Get connector from Cosmos by Alias
        /// </summary>
        /// <returns>Connector document.</returns> 
        public async Task<ConnectorModel> GetConnectorByAlias(string alias, bool includeDataSource = false)
        {
            var connectorDocument = _connectors?.Where(connectorDocument => !string.IsNullOrEmpty(connectorDocument.Alias) && connectorDocument.Alias.Equals(alias, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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
            var connectorDocuments = _connectors.AsQueryable().Where(condition);
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
            var dataSourceDocument = _dataSources.First(x=>x.Id == dataSourceId);
            return _mapper.Map<DataSourceModel>(dataSourceDocument);
        }

        /// <summary>
        /// Fetch all data sources from Cosmos
        /// </summary>
        /// <returns>List of data sources.</returns> 
        public async Task<IEnumerable<DataSourceModel>> GetDataSources(Expression<Func<DataSourceDocument, bool>> condition = null)
        {
            var dataSourceDocuments = _dataSources.AsQueryable().Where(condition);
            List<DataSourceModel> dataSources = _mapper.Map<List<DataSourceModel>>(dataSourceDocuments);
            return dataSources;
        }

        /// <summary>
        /// Write connector document to cosmos
        /// </summary>
        public async Task StoreConnector(ConnectorDocument connectorDocument)
        {
            //no op
        }

        /// <summary>
        /// Write data source document to cosmos
        /// </summary>
        public async Task StoreDataSource(DataSourceDocument dataSourceDocument)
        {
            //no op
        }

        /// <summary>
        /// Write connector registration document to cosmos
        /// </summary>
        public async Task StoreConnectorRegistration(ConnectorRegistrationDocument connectorRegistrationDocument)
        {
            //no op

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