using System;
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
        /// Flag to indicate if query parameters in POST request must be skiped. True fot TT connectors
        /// </summary>
        public bool SkipQueryParams { get; set; }
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
        public List<ConnectorRequestParameter> Parameters { get; set; }
        /// <summary>
        /// Sorting properties
        /// </summary>
        public ConnectorRequestSorting Sorting { get; set; }
    }

    /// <summary>
    /// Request parameter definition
    /// </summary>
    public class ConnectorRequestParameter
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
        /// </summary>s
        public object DefaultValue { get; set; }
        /// <summary>
        /// Name of querys tring parameter in request
        /// </summary>
        public string CdmName { get; set; }
        /// <summary>
        /// Tags for the related to the parameter. Ex: pagination/ sort etc.
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// Key being used in Request param. Currently only used with OData.
        /// </summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// ConnectorRequestSorting
    /// </summary>
    public class ConnectorRequestSorting
    {
        public List<ConnectorRequestSortingProperties> Properties { get; set; } = new List<ConnectorRequestSortingProperties>();
    }

    /// <summary>
    /// ConnectorRequestSortingProperties
    /// </summary>
    public class ConnectorRequestSortingProperties
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
        /// <summary>
        /// Indicates default sort type if any property was given.
        /// </summary>
        public bool? IsDefault { get; set; }
    }
}
