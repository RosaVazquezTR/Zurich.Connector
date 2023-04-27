using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    /// <summary>
    /// Stores details about the connector response.
    /// </summary>
    public class ConnectorResponse
    {
        /// <summary>
        /// Define connector response schema
        /// </summary>
        public ConnectorReponseSchema Schema { get; set; }

        /// <summary>
        /// Define connector response Type
        /// </summary>
        public string Type { get; set; }

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
        /// Define connector transform location
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
    public class ConnectorReponseSchema
    {
        /// <summary>
        /// Connector response properties
        /// </summary>
        public ConnectorReponseProperties properties { get; set; }
    }

    /// <summary>
    /// Definition of connector response property.
    /// </summary>
    public class ConnectorReponseProperties
    {
        public string property { get; set; }
    }
}
