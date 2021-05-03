using Microsoft.Extensions.Logging;
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
    /// <summary>
    /// Service to get mapping service
    /// </summary>
    public interface IDataMappingFactory
    {
        /// <summary>
        /// Gets the service needed to map data
        /// </summary>
        /// <param name="dataMappingInformation">Information used for data mapping <see cref="DataMappingClass"/></param>
        /// <returns>IDataMapping service used to map the results</returns>
        IDataMapping GetMapper(DataMappingClass dataMappingInformation);
    }
    public class DataMappingFactory : IDataMappingFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataMappingFactory> _logger;

        public DataMappingFactory(IServiceProvider serviceProvider, ILogger<DataMappingFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IDataMapping GetMapper(DataMappingClass dataMappingInformation)
        {
            if (dataMappingInformation.AuthType == AuthType.TransferToken)
            {
                DataMappingTransfer service = (DataMappingTransfer)_serviceProvider.GetService(typeof(DataMappingTransfer)); 
                return service;
            } 
            else if (dataMappingInformation.AuthType == AuthType.OAuth)
            {
                DataMappingOAuth service = (DataMappingOAuth)_serviceProvider.GetService(typeof(DataMappingOAuth));
                return service;
            }

            return null;
        }
    }
}
