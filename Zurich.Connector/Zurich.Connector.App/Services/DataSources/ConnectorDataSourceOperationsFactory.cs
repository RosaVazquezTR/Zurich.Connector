using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Zurich.Connector.Data;

namespace Zurich.Connector.App.Services.DataSources
{
    public interface IConnectorDataSourceOperationsFactory
    {
        /// <summary>
        /// Instantiates a data source operations provider
        /// </summary>
        /// <param name="appCode">The data source app code</param>
        /// <returns></returns>
        IConnectorDataSourceOperations GetDataSourceOperationsService(string appCode);
    }
    /// <summary>
    /// Represent a factory responsible with instantiating the appropriate data source operations provider
    /// </summary>
    public class ConnectorDataSourceOperationsFactory: IConnectorDataSourceOperationsFactory
    {
        private readonly IEnumerable<IConnectorDataSourceOperations> _serviceImplementations;

        public ConnectorDataSourceOperationsFactory(IEnumerable<IConnectorDataSourceOperations> implementations)
        {
            _serviceImplementations = implementations;
        }

        public IConnectorDataSourceOperations GetDataSourceOperationsService(string appCode)
        {
            return _serviceImplementations.Where(si => si.IsCompatible(appCode)).SingleOrDefault();
        }
    }
}
