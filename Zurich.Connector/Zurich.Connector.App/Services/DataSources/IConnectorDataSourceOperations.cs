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
        dynamic SetItemLink(EntityType entityType, dynamic item, string hostName);

        /// <summary>
        /// Checks if the service implementation is compatible with the third party app
        /// </summary>
        /// <param name="appCode">The third party app code</param>
        bool IsCompatible(string appCode);
    }
}
