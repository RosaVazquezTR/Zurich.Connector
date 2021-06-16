using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// Store connector details
    /// </summary>
    public class ConnectorModelEntity
    {
        /// <summary>
        /// Connector ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Connector Info like title, description etc.
        /// </summary>
        public ConnectorInfoModelEntity Info { get; set; }
        /// <summary>
        /// Store the data source details.
        /// </summary>
        public DataSourceModelEntity DataSource { get; set; }
        /// <summary>
        /// Request details
        /// </summary>
        public ConnectorRequestModelEntity Request { get; set; }
        /// <summary>
        /// Host name
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Cdm Mapping
        /// </summary>
        public CdmMappingEntity CdmMappingEntity { get; set; }
    }

    /// <summary>
    /// Store connector info
    /// </summary>
    public class ConnectorInfoModelEntity
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
    public class CdmMappingEntity
    {
        public StructuredEntity StructuredEntity { get; set; }

        public UnstructuredEntity UnstructuredEntity { get; set; }
    }

    public class StructuredEntity
    {
        public List<CdmElementEntity> CdmElementEntity { get; set; }
    }

    public class CdmElementEntity
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
        public string _Comment { get; set; }
    }

    public class UnstructuredEntity
    {
        public CdmElementEntity DataElementEntity { get; set; }
    }
}
