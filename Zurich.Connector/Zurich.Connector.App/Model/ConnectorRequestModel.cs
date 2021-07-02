using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Stores details about the connector request.
    /// </summary>
    public class ConnectorRequestModel
    {
        /// <summary>
        /// End point path. Ex: "/api/6/sites",
        /// </summary>
        public string EndpointPath { get; set; }
        /// <summary>
        /// Method type. Ex: "get"
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// Reponse Content Type Ex: JSON, XML etc.
        /// </summary>
        public ResponseContentType? responseContentType { get; set; }
        /// <summary>
        /// Request parameters
        /// </summary>
        public List<ConnectorRequestParameterModel> Parameters { get; set; }
    }

    /// <summary>
    /// Request parameter definition
    /// </summary>
    public class ConnectorRequestParameterModel
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Use "Query" or "Header"
        /// </summary>
        public string InClause { get; set; }
        /// <summary>
        /// Specify if the parameter is a required property.
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Type of the parameter
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Default value of the parameter
        /// </summary>
        public object DefaultValue { get; set; }
        /// <summary>
        /// Name of querys tring parameter in request
        /// </summary>
        public string CdmName { get; set; }
    }
}
