using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// Stores details about the connector request.
    /// </summary>
    public class ConnectorRequestViewModel
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
        /// Request parameters
        /// </summary>
        public List<ConnectorRequestParameterViewModel> Parameters { get; set; }
        /// <summary>
        /// Sorting properties
        /// </summary>
        public ConnectorRequestSortingViewModel Sorting { get; set; }
    }

    /// <summary>
    /// Request parameter definition
    /// </summary>
    public class ConnectorRequestParameterViewModel
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
        /// Name of querys string parameter in request
        /// </summary>
        public string CdmName { get; set; }
        /// <summary>
        /// Key being used in Request param. Currently only used with OData.
        /// </summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// ConnectorRequestSortingViewModel
    /// </summary>
    public class ConnectorRequestSortingViewModel
    {
        public List<ConnectorRequestSortingPropertiesViewModel> Properties { get; set; }
    }

    /// <summary>
    /// ConnectorRequestSortingPropertiesViewModel
    /// </summary>
    public class ConnectorRequestSortingPropertiesViewModel
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
