using System.Collections.Generic;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    /// <summary>
    /// Stores details about the connector request.
    /// </summary>
    public class ConnectorRequest
    {
        /// <summary>
        /// End point path. Ex: "/api/6/sites",
        /// </summary>
        public string endpointPath { get; set; }
        /// <summary>
        /// Method type. Ex: "get"
        /// </summary>
        public string method { get; set; }
        /// <summary>
        /// Reponse Content Type Ex: JSON, XML etc.
        /// </summary>
        public ResponseContentType? responseContentType { get; set; }
        /// <summary>
        /// Request parameters
        /// </summary>
        public List<ConnectorRequestParameter> parameters { get; set; }
    }

    /// <summary>
    /// Request parameter definition
    /// </summary>
    public class ConnectorRequestParameter
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Use "Query" or "Header"
        /// </summary>
        public string inClause { get; set; }
        /// <summary>
        /// Specify if the parameter is a required property.
        /// </summary>
        public bool required { get; set; }
        /// <summary>
        /// Type of the parameter
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Default value of the parameter
        /// </summary>
        public object defaultValue { get; set; }
        /// <summary>
        /// Name of querys tring parameter in request
        /// </summary>
        public string cdmname { get; set; }
    }
}
