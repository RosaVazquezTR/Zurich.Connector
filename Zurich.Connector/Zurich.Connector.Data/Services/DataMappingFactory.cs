using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.Data.Services
{
    public class DataMappingFactory 
    {
        private IDataMappingRepository _dataMappingRepository;
        private readonly IServiceProvider _serviceProvider;

        public DataMappingFactory(IServiceProvider serviceProvider, IDataMappingRepository dataMappingRepository)
        {
            _serviceProvider = serviceProvider;
            _dataMappingRepository = dataMappingRepository;
        }

        public async Task<dynamic> GetResults(string appCode, DataType dataType)
        {
            DataMappingClass dataTypeInformation = await _dataMappingRepository.GetMap(appCode, dataType);

            if (dataTypeInformation.AuthType == "transferToken")
            {

            }
            return null;
        }
    }
}
