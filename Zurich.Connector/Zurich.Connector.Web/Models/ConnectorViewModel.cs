using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Web.Enum;
using Zurich.Connector.App.Enum;

namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// To store detailed connector information.
    /// </summary>
    public class ConnectorViewModel
    {
        /// <summary>
        /// Connector ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The connector alias
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// Connector Info like title, description etc.
        /// </summary>
        public ConnectorDetailsInfoViewModel Info { get; set; }
        /// <summary>
        /// Store the data source details.
        /// </summary>
        public DataSourceViewModel DataSource { get; set; }
        /// <summary>
        /// Request details
        /// </summary>
        public ConnectorRequestViewModel Request { get; set; }
        /// <summary>
        /// Response details
        /// </summary>
        public ConnectorResponseViewModel Response { get; set; }
        /// <summary>
        /// Filter details
        /// </summary>
        public List<FilterViewModel> Filters { get; set; }
        /// <summary>
        /// CDM Mapping of properties
        /// </summary>
        public CDMMappingViewModel CDMMapping { get; set; }
        /// <summary>
        /// Determines whether to show pre release connectors 
        /// </summary>
        public bool PreRelease { get; set; }
        /// <summary>
        /// gets or sets the EntityType
        /// </summary>
        public string EntityType { get; set; }
        /// <summary>
        /// Sort type options
        /// </summary>
        public IEnumerable<SortType> Sort { get; set; }

        /// <summary>
        /// Registration status
        /// </summary>
        public RegistrationStatus RegistrationStatus { get; set; }
        /// <summary>
        /// Determines if the connector requires to call oauth to get an authentication token
        /// </summary>
        public bool BasicAuthentication { get; set; }
    }

    /// <summary>
    /// Store connector info
    /// </summary>
    public class ConnectorDetailsInfoViewModel
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
    }
}
