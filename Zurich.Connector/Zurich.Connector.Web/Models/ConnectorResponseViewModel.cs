using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// Stores details about the connector response.
    /// </summary>
    public class ConnectorResponseViewModel
    {
        /// <summary>
        /// Define connector response schema
        /// </summary>
        public ConnectorReponseSchemaViewModel Schema { get; set; }
    }

    /// <summary>
    /// Define connector response schema
    /// </summary>
    public class ConnectorReponseSchemaViewModel
    {
        /// <summary>
        /// Connector response properties
        /// </summary>
        public ConnectorReponsePropertiesViewModel Properties { get; set; }
    }

    /// <summary>
    /// Definition of connector response property.
    /// </summary>
    public class ConnectorReponsePropertiesViewModel
    {
        public string Property { get; set; }
    }
}
