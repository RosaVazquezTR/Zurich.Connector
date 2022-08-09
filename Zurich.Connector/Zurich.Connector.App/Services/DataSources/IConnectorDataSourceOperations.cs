using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    /// <summary>
    /// Represents a service responsible with handling data source specific operations
    /// </summary>
    public interface IConnectorDataSourceOperations
    {
        /// <summary>
        /// Sets the link to the item's location in the data source application
        /// </summary>
        /// <param name="entityType">The item data type</param>
        /// <param name="item">The target data item</param>
        /// <param name="hostName">The data source app host name</param>
        /// <returns></returns>
        Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName);

        /// <summary>
        /// Sets the sort parameters if any special condition exists in the data source application based on other parameters
        /// </summary>
        /// <param name="allParameters">All final parameters that will be sent to the request</param>
        /// <returns></returns>
        Task<Dictionary<string, string>> SetSortParameters(Dictionary<string, string> allParameters);

        /// <summary>
        /// Checks if the service implementation is compatible with the third party app
        /// </summary>
        /// <param name="appCode">The third party app code</param>
        bool IsCompatible(string appCode);
    }
}
