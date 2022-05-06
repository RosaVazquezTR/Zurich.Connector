using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using AutoMapper;
using System.Collections.Generic;

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
        /// <param name="connectionIdentifier">Connection Id</param>
        /// <param name="hostname">Host name</param>
        /// <returns> Returns ConnectorModel</returns>
        public Task<ConnectorModel> RetrieveProductInformationMap(string connectionIdentifier, string hostname, bool retrieveFilters);

        public Dictionary<string, string> UpdateOffset(Dictionary<string, string> queryParameters)
        {
            return queryParameters;
        }
    }

    /// <summary>
    /// DataMappingService used to modify data mapping requests and responses
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
        /// <param name="connectionIdentifier">Connection Id or Alias</param>
        /// <param name="hostname">Host name</param>
        /// <returns> Returns ConnectorModel</returns>
        public async virtual Task<ConnectorModel> RetrieveProductInformationMap(string connectionIdentifier, string hostname, bool retrieveFilters)
        {
            ConnectorModel connector;

            if (int.TryParse(connectionIdentifier, out var _))
            {
                // This means we passed connector id
                connector = await _cosmosService.GetConnector(connectionIdentifier, true);
            }
            else 
            {
                // This means we passed connector alias
                connector = await _cosmosService.GetConnectorByAlias(connectionIdentifier, true); ;
            }

            if (connector != null)
            {
                connector.HostName = hostname;
            }

            return connector;
        }

    }
}
