
using System.Collections.Generic;
using Newtonsoft.Json;
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
        [JsonProperty("partitionkey")]
        public string PartitionKey { get { return CosmosConstants.ConnectorPartitionKey; } }
        /// <summary>
        /// Connector Info like title, description etc.
        /// </summary>
        public ConnectorInfo Info { get; set; }
        /// <summary>
        /// Request details
        /// </summary>
        public ConnectorRequest Request { get; set; }
        /// <summary>
        /// Response details
        /// </summary>
        public ConnectorResponse Response { get; set; }
        /// <summary>
        /// Filter details
        /// </summary>
        public List<ConnectorFilter> Filters { get; set; }
        /// <summary>
        /// Pagination details.
        /// </summary>
        public PaginationInfo Pagination { get; set; }
        /// <summary>
        /// CDM Mapping of properties
        /// </summary>
        public CDMMapping CdmMapping { get; set; }
        /// <summary>
        /// DataSource Document
        /// </summary>
        public DataSourceDocument DataSource { get; set; }
        /// <summary>
        /// Result Location
        /// </summary>
        public string ResultLocation { get; set; }
        /// <summary>
        /// Host name
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Determines whether to show pre release connectors 
        /// </summary>
        public bool PreRelease { get; set; }
    }

    public class ConnectorInfo
    {
        /// <summary>
        /// Connector title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Connector description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Entity type like Document, Search etc.
        /// </summary>
        public ConnectorEntityType EntityType { get; set; }
        /// <summary>
        /// Data Source ID
        /// </summary>
        public string DataSourceId { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Flag to indicate if a dynamic filter is available
        /// </summary>
        public bool? IsDynamicFilter { get; set; }
        /// <summary>
        /// Used to determine if a child or Parent
        /// </summary>
        public string SubType { get; set; }
    }

    /// <summary>
    /// Pagination properties.
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// Indicates id pagination is available.
        /// </summary>
        public bool Available { get; set; }
        /// <summary>
        /// Pagination type.
        /// </summary>
        public string PaginationType { get; set; }
        /// <summary>
        /// Set true if the offset indexing is zero based.
        /// </summary>
        public bool IsZeroBasedOffset { get; set; }
    }
}
