using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Stores details about the connector response.
    /// </summary>
    public class ConnectorResponseModel
    {
        /// <summary>
        /// Define connector response schema
        /// </summary>
        public ConnectorReponseSchemaModel Schema { get; set; }
    }

    /// <summary>
    /// Define connector response schema
    /// </summary>
    public class ConnectorReponseSchemaModel
    {
        /// <summary>
        /// Connector response properties
        /// </summary>
        public ConnectorReponsePropertiesModel Properties { get; set; }
    }

    /// <summary>
    /// Definition of connector response property.
    /// </summary>
    public class ConnectorReponsePropertiesModel
    {
        public string Property { get; set; }
    }
}
