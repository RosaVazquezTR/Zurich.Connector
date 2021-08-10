
using System.Collections.Generic;
using Zurich.Common.Cosmos;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    /// <summary>
    /// Store connector details
    /// </summary>
    // Reference - https://trten-my.sharepoint.com/:w:/g/personal/vassil_vassilev_thomsonreuters_com/EUbDYj4Yi1hKnSft6DgfzZQBxTKt07JW-LlDrMBo4VbgXQ
    public class ConnectorDocument: CosmosDocument
    {
        public ConnectorDocument() : base(CosmosConstants.ConnectorPartitionKey) { }
        /// <summary>
        /// Connector ID
        /// </summary>
        public override string Id { get; set; }
        /// <summary>
        /// Connector Alias in the format of [appcode].[entitytype].[subtype?] for e.g. msgraph.document.recent, plcca.search
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// Connector partition key
        /// </summary>
        public string partitionkey { get { return CosmosConstants.ConnectorPartitionKey; } }
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
        /// Pagination details.
        /// </summary>
        public PaginationInfo pagination { get; set; }
        /// <summary>
        /// CDM Mapping of properties
        /// </summary>
        public CDMMapping cdmMapping { get; set; }
        /// <summary>
        /// DataSource Document
        /// </summary>
        public DataSourceDocument dataSource { get; set; }
        /// <summary>
        /// Result Location
        /// </summary>
        public string resultLocation { get; set; }
        /// <summary>
        /// Host name
        /// </summary>
        public string hostName { get; set; }
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
        public EntityType entityType { get; set; }
        /// <summary>
        /// Data Source ID
        /// </summary>
        public string dataSourceId { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// Flag to indicate if a dynamic filter is available
        /// </summary>
        public bool? isDynamicFilter { get; set; }
    }

    /// <summary>
    /// Pagination properties.
    /// </summary>
    public class PaginationInfo
    {
        // Indicates id pagination is available.
        public bool available { get; set; }
        // Pagination type.
        public string paginationType { get; set; }
        // Set true if the offset indexing is zero based.
        public bool isZeroBasedOffset { get; set; }
    }
}
