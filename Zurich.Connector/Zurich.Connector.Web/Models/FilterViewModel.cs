using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// Class that represents the Filters defined in the connector document
    /// </summary>
    public class FilterViewModel
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
        /// Define the source of filters
        /// </summary>
        public List<FilterListViewModel> FilterList { get; set; }
    }

    public class FilterListViewModel
    {
        /// <summary>
        /// Name of the Sub filter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Id of the filter
        /// </summary>
        public string Id { get; set; }
    }
}
