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
}
