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
