using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.Data.Services
{
    public class DataMappingFactory 
    {
        private IDataMappingRepository _dataMappingRepository;
        private IServiceProvider _serviceProvider;

        public DataMappingFactory(IServiceProvider serviceProvider, IDataMappingRepository dataMappingRepository)
        {
            _serviceProvider = serviceProvider;
            _dataMappingRepository = dataMappingRepository;
        }

        public async Task<dynamic> GetResults(string appCode, DataType dataType, string transferToken)
        {
            DataMappingClass dataTypeInformation = await _dataMappingRepository.GetMap(appCode, dataType);
            dynamic results = null;
            if (dataTypeInformation.AuthType == "transferToken")
            {
                DataMappingTransfer service = (DataMappingTransfer)_serviceProvider.GetService(typeof(DataMappingTransfer));
                results = await service.Get<dynamic>("PLCUS", DataType.History, dataTypeInformation, transferToken);  
            } 
            else if (dataTypeInformation.AuthType == "OAuth")
            {
                DataMappingOAuth service = (DataMappingOAuth)_serviceProvider.GetService(typeof(DataMappingOAuth));
                results = await service.Get<dynamic>(appCode, dataType, dataTypeInformation, null);
            }

            return results;
        }
    }
}
