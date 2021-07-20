using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Zurich.Connector.App.Model;
using AutoMapper;

namespace Zurich.Connector.App.Services
{
    /// <summary>
    /// Interface implemented by DataMappingService
    /// </summary>
    public interface IDataMappingService
    {
        /// <summary>
        /// Gets data from cosmo db using Cosmo service and reorganizes the data
        /// </summary>
        /// <param name="connectionId">Connection Id</param>
        /// <param name="hostname">Host name</param>
        /// <returns> Returns ConnectorModel</returns>
        public Task<ConnectorModel> RetrieveProductInformationMap(string connectionId, string hostname, bool retrievefilters);
    }

    /// <summary>
    /// DataMappingService gets data from cosmo db using Cosmo service and reorganizes the data
    /// </summary>
    public class DataMappingService : IDataMappingService
    {
        private readonly ICosmosService _cosmosService;
        private readonly IMapper _mapper;

        public DataMappingService(ICosmosService cosmosService, IMapper mapper)
        {

            _cosmosService = cosmosService;
            _mapper = mapper;

        }

        /// <summary>
        /// Gets data from cosmo db using Cosmo service and reorganizes the data
        /// </summary>
        /// <param name="connectionId">Connection Id</param>
        /// <param name="hostname">Host name</param>
        /// <returns> Returns ConnectorModel</returns>
        public async virtual Task<ConnectorModel> RetrieveProductInformationMap(string connectionId, string hostname, bool retrievefilters)
        {
            ConnectorModel connectorModel = await _cosmosService.GetConnector(connectionId);

            string dataSourceId = connectorModel?.Info?.DataSourceId;

            DataSourceModel dataSourceModel = null;

            if (!string.IsNullOrEmpty(dataSourceId))
            {
                dataSourceModel = await _cosmosService.GetDataSource(dataSourceId);
            }

            connectorModel.DataSource = dataSourceModel;

            if (connectorModel != null)
            {
                connectorModel.HostName = hostname;
            }

            return connectorModel;
        }

    }
}
