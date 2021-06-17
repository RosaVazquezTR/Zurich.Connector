using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

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
        public List<ConnectorFilterViewModel> Filters { get; set; }
        /// <summary>
        /// CDM Mapping of properties
        /// </summary>
        public CDMMappingViewModel CDMMapping { get; set; }
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
        public EntityType EntityType { get; set; }
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
