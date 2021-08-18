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
        public string Name { get; set; }
        /// <summary>
        /// Description about the filter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Flag for Selection of filter type
        /// </summary>
        public string IsMultiSelect { get; set; }

        /// <summary>
        /// Mapping for request parameter
        /// </summary>
        public string RequestParameter { get; set; }

        /// <summary>
        /// Define the Sub filters
        /// </summary>
        public List<FilterList> FilterList { get; set; }
    }

    public class FilterList
        {
        /// <summary>
        /// Name of the subfilter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Id of the filter
        /// </summary>
        public string Id { get; set; }
    }

    // TODO: Can the models below be cleaned up if they are not being used?

    /// <summary>
    /// Define the source of filters. There can be 3 types of sources
    /// 1- Define the filter options as part of an API response (Not supported currently)
    /// 2- Get filter sources from an end point (ex: PL UK filters)
    /// 3- Flat list of key-value pairs (ex: O365 filters)
    /// </summary>
    public class FilterSource
    {
        public string ResponseElement { get; set; }
        /// <summary>
        /// Define the filter options as part of an API response (Not supported currently)
        ///// </summary>
        public FilterResponse Response { get; set; }
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
    public class FilterResponse
    {
        public string ResponseProperty { get; set; }
        public string FilterCDM { get; set; }
    }

    /// <summary>
    /// Mapping of filter request parameters.
    /// </summary>
    public class MappingRequestParameter
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
