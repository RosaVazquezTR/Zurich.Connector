using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Filter details associated with the connector.
    /// </summary>
    /// TODO: Should this be removed ?
    public class ConnectorFilterModel
    {
        /// <summary>
        /// Name of the filter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description about the filter
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Define the source of filters
        /// </summary>
        public FilterSourceModel Source { get; set; }
        /// <summary>
        /// Mapping for request parameter
        /// </summary>
        public string RequestParameter { get; set; }
    }

    /// <summary>
    /// Define the source of filters. There can be 3 types of sources
    /// 1- Define the filter options as part of an API response (Not supported currently)
    /// 2- Get filter sources from an end point (ex: PL UK filters)
    /// 3- Flat list of key-value pairs (ex: O365 filters)
    /// </summary>
    public class FilterSourceModel
    {
        public string ResponseElement { get; set; }
        /// <summary>
        /// Define the filter options as part of an API response (Not supported currently)
        ///// </summary>
        public FilterResponseModel Response { get; set; }
        ///// <summary>
        ///// Get filter sources from an end point (ex: PL UK filters)
        ///// </summary>
        public string EndPointPath { get; set; }
        ///// <summary>
        ///// Flat list of key-value pairs (ex: O365 filters)
        ///// </summary>
        public List<KeyValuePair<string, string>> KeyValue { get; set; }
    }

    /// <summary>
    /// Mapping of filter options as part of an API response
    /// </summary>
    public class FilterResponseModel
    {
        /// <summary>
        /// Response Property
        /// </summary>
        public string ResponseProperty { get; set; }
        /// <summary>
        /// Filter CDM
        /// </summary>
        public string FilterCDM { get; set; }
    }

    /// <summary>
    /// Mapping of filter request parameters.
    /// </summary>
    public class MappingRequestParameterModel
    {
        /// <summary>
        /// Name of the filter property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Specify Query or Header
        /// </summary>
        public string InClause { get; set; }
        /// <summary>
        /// Type of the filter property
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Default value of the filter property.
        /// </summary>
        public string DefaultValue { get; set; }

    }
}
