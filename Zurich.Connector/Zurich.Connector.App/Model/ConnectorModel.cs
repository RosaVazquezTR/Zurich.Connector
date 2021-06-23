using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Store connector details
    /// </summary>
    public class ConnectorModel
    {
        /// <summary>
        /// Connector ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Connector Info like title, description etc.
        /// </summary>
        public ConnectorInfoModel Info { get; set; }
        /// <summary>
        /// Store the data source details.
        /// </summary>
        public DataSourceModel DataSource { get; set; }
        /// <summary>
        /// Request details
        /// </summary>
        public ConnectorRequestModel Request { get; set; }
        /// <summary>
        /// Host name
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Cdm Mapping
        /// </summary>
        public CdmMapping CdmMapping { get; set; }
        /// <summary>
        /// Result Location
        /// </summary>
        public string ResultLocation { get; set; }
    }

    /// <summary>
    /// Store connector info
    /// </summary>
    public class ConnectorInfoModel
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

    /// <summary>
    /// Stores Cdm Mapping
    /// </summary>
    public class CdmMapping
    {
        public List<CdmElement> Structured { get; set; }
        public List<CdmElement> Unstructured { get; set; }
    }

    public class CdmElement
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of the property
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Name of the response element
        /// </summary>
        public string ResponseElement { get; set; }
        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; }
    }

    
}
