using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    /// <summary>
    /// Stores details about filters.
    /// </summary>
    public class ConnectorFilter
    {
        /// <summary>
        /// Name of the filter
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Description about the filter
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Define the source of filters
        /// </summary>
        public List<FilterSource> sources { get; set; }
        /// <summary>
        /// Mapping for request parameter
        /// </summary>
        public MappingRequestParameter mappingRequestParameter {get; set; }
    }
    /// <summary>
    /// Define the source of filters. There can be 3 types of sources
    /// 1- Define the filter options as part of an API response (Not supported currently)
    /// 2- Get filter sources from an end point (ex: PL UK filters)
    /// 3- Flat list of key-value pairs (ex: O365 filters)
    /// </summary>
    public class FilterSource
    {
        /// <summary>
        /// Define the filter options as part of an API response (Not supported currently)
        /// </summary>
        public FilterResponse response { get; set; }
        /// <summary>
        /// Get filter sources from an end point (ex: PL UK filters)
        /// </summary>
        public string endPointPath { get; set; }
        /// <summary>
        /// Flat list of key-value pairs (ex: O365 filters)
        /// </summary>
        public List<KeyValuePair<string, string>> keyValue { get; set; }
    }

    /// <summary>
    /// Mapping of filter options as part of an API response
    /// </summary>
    public class FilterResponse
    {
        public string responseProperty { get; set; }
        public string filterCDM { get; set; }
    }

    /// <summary>
    /// Mapping of filter request parameters.
    /// </summary>
    public class MappingRequestParameter
    {
        /// <summary>
        /// Name of the filter property
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Specify Query or Header
        /// </summary>
        public string inClause { get; set; }
        /// <summary>
        /// Type of the filter property
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Default value of the filter property.
        /// </summary>
        public string defaultValue { get; set; }

    }
}
