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
    public interface IDataMappingService
    {
        public Task<ConnectorModelEntity> RetrieveProductInformationMap(string connectionId, string hostname);
    }

    public class DataMappingService : IDataMappingService
    {
        private readonly ICosmosService _cosmosService;
        private readonly IMapper _mapper;

        public DataMappingService(ICosmosService cosmosService, IMapper mapper)
        {

            _cosmosService = cosmosService;
            _mapper = mapper;

        }

        public async virtual Task<ConnectorModelEntity> RetrieveProductInformationMap(string connectionId, string hostname)
        {
            ConnectorModel connectorModel = await _cosmosService.GetConnector(connectionId);

            string dataSourceId = connectorModel?.Info?.DataSourceId;

            ConnectorModelEntity connectorModelEntity = _mapper.Map<ConnectorModelEntity>(connectorModel);
            DataSourceModel dataSourceModel = null;
            DataSourceModelEntity dataSourceModelEntity = null;

            if (!string.IsNullOrEmpty(dataSourceId))
            {
                dataSourceModel = await _cosmosService.GetDataSource(dataSourceId);
                dataSourceModelEntity = _mapper.Map<DataSourceModelEntity>(dataSourceModel);
            }

            connectorModelEntity.DataSource = dataSourceModelEntity;

            if (connectorModelEntity != null)
            {
                connectorModelEntity.HostName = hostname;
            }

            return connectorModelEntity;
        }

    }
}
