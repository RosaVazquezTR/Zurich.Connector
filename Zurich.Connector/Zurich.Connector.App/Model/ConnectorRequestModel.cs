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
        public ResponseContentType? ResponseContentType { get; set; }
        /// <summary>
        /// Indicates if there is any array in the xml. 
        /// </summary>
        public string XmlArrayAttribute { get; set; }
        /// <summary>
        /// Request parameters
        /// </summary>
        public List<ConnectorRequestParameterModel> Parameters { get; set; }
        /// <summary>
        /// Sorting properties
        /// </summary>
        public ConnectorRequestSortingModel Sorting { get; set; }
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
        public string DefaultValue { get; set; }
        /// <summary>
        /// Name of querys tring parameter in request
        /// </summary>
        public string CdmName { get; set; }
        /// <summary>
        /// Tags related to the parameter. Ex: Pagination/ filter etc.
        /// </summary>
        public List<string> Tags { get; set; }

    }

    /// <summary>
    /// ConnectorRequestSortingModel
    /// </summary>
    public class ConnectorRequestSortingModel
    {
       public List<ConnectorRequestSortingPropertiesModel> Properties { get; set; }
    }

    /// <summary>
    /// ConnectorRequestSortingPropertiesModel
    /// </summary>
    public class ConnectorRequestSortingPropertiesModel
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Element
        /// </summary>
        public string Element { get; set; }
        /// <summary>
        /// Element Value
        /// </summary>
        public string ElementValue { get; set; }
    }
}
