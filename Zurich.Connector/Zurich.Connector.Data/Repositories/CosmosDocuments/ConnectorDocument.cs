
using System.Collections.Generic;
using Zurich.Common.Cosmos;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    /// <summary>
    /// Store connector details
    /// </summary>
    // Reference - https://trten-my.sharepoint.com/:w:/g/personal/vassil_vassilev_thomsonreuters_com/EUbDYj4Yi1hKnSft6DgfzZQBxTKt07JW-LlDrMBo4VbgXQ
    public class ConnectorDocument: CosmosDocument
    {
        public ConnectorDocument() : base(CosmosConstants.ConnectorList) { }
        /// <summary>
        /// Connector ID
        /// </summary>
        public override string Id { get; set; }
        public string partitionkey { get { return CosmosConstants.ConnectorList; } }
        /// <summary>
        /// Connector Info like title, description etc.
        /// </summary>
        public ConnectorInfo info { get; set; }
        /// <summary>
        /// Request details
        /// </summary>
        public ConnectorRequest request { get; set; }
        /// <summary>
        /// Response details
        /// </summary>
        public ConnectorResponse response { get; set; }
        /// <summary>
        /// Filter details
        /// </summary>
        public List<ConnectorFilter> filters { get; set; }
        /// <summary>
        /// CDM Mapping of properties
        /// </summary>
        public List<CDMMapping> cdmMapping { get; set; }
    }

    public class ConnectorInfo
    {
        /// <summary>
        /// Connector title
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Connector description
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Entity type like Document, Search etc.
        /// </summary>
        public string entityType { get; set; }
        /// <summary>
        /// Data Source ID
        /// </summary>
        public string dataSourceId { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public string version { get; set; }
    }
}
