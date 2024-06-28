using System;
using Zurich.Connector.Data.DataMap;

namespace Zurich.Connector.Data.Factories
{
    /// <summary>
    /// Service to get mapping service
    /// </summary>
    public interface IDataMappingFactory
    {
        /// <summary>
        /// Gets the service needed to map data
        /// </summary>
        /// <returns>IDataMapping service used to map the results</returns>
        IDataMapping GetImplementation(string dataMappingType);
    }

    public class DataMappingFactory : IDataMappingFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DataMappingFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IDataMapping GetImplementation(string dataMappingType)
        {
            if (dataMappingType.Equals("oauth2", StringComparison.OrdinalIgnoreCase))
                return (IDataMapping)serviceProvider.GetService(typeof(DataMappingOAuth));
            else if (dataMappingType.Equals("transferToken", StringComparison.OrdinalIgnoreCase))
                return (IDataMapping)serviceProvider.GetService(typeof(DataMappingTransfer));
            else if (dataMappingType.Equals("basic", StringComparison.OrdinalIgnoreCase))
                return (IDataMapping)serviceProvider.GetService(typeof(DataMappingBasic));
            else if (dataMappingType.Equals("oauth2AndTransferToken", StringComparison.OrdinalIgnoreCase))
                return (IDataMapping)serviceProvider.GetService(typeof(DataMappingOAuthWithTransfer));

            return (IDataMapping)serviceProvider.GetService(typeof(DataMapping));
        }
    }
}
