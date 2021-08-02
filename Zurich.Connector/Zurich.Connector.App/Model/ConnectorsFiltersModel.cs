using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.App.Model
{
    public class ConnectorsFiltersModel
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
        public List<FilterListModel> FilterList { get; set; }
    }
    public class FilterListModel
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
