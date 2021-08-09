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
        /// Indicates if there is any array in the xml. 
        /// </summary>
        public string xmlArrayAttribute { get; set; }
        /// <summary>
        /// Request parameters
        /// </summary>
        public List<ConnectorRequestParameter> parameters { get; set; }
        /// <summary>
        /// Sorting properties
        /// </summary>
        public ConnectorRequestSorting sorting { get; set; }
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
        /// <summary>
        /// Tags for the related to the parameter. Ex: pagination/ sort etc.
        /// </summary>
        public List<string> tags { get; set; }
        /// <summary>
        /// Key being used in Request param. Currently only used with OData.
        /// </summary>
        public string key { get; set; }
    }

    /// <summary>
    /// ConnectorRequestSorting
    /// </summary>
    public class ConnectorRequestSorting
    {
        public List<ConnectorRequestSortingProperties> properties { get; set; }
    }

    /// <summary>
    /// ConnectorRequestSortingProperties
    /// </summary>
    public class ConnectorRequestSortingProperties
    {
        /// <summary>
        /// Name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Element
        /// </summary>
        public string element { get; set; }
        /// <summary>
        /// Element Value
        /// </summary>
        public string elementValue { get; set; }
    }
}
