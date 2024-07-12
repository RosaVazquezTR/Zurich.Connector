using System.Collections.Generic;
using Zurich.Connector.Data.Model;

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

        /// <summary>
        /// Define connector response Type
        /// </summary>
        public ResponseContentType? Type { get; set; }

        /// <summary>
        /// Whether or not the connector response needs to be sorted on our side
        /// </summary>
        public bool? UseInternalSorting { get; set; }

        /// <summary>
        /// Whether or not the connector JSON response needs to be transformed using JUST library
        /// </summary>
        public bool? UseJsonTransformation { get; set; }

        /// <summary>
        /// Whether or not the connector uses the IH permissions check workarround
        /// </summary>
        public bool? UsePermissionsCheck { get; set; }

        /// <summary>
        /// Define connector XSLT transform record
        /// </summary>
        public string TransformationLocation { get; set; }

        /// <summary>
        /// Indicates if there is any array in the xml. 
        /// </summary>
        public IEnumerable<string> XmlArrayAttribute { get; set; }
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
