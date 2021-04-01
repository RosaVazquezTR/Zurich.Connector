using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// Class that stream lines some of the API call information
    /// </summary>
    public class DataMappingClass
    {
        /// <summary>
		/// The code used for the application we are calling
		/// </summary>
        public string AppCode { get; set; }

        /// <summary>
        /// The DataType of a given request
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Information about how to call the api
        /// </summary>
        public DataMappingApiRequest Api { get; set; }

        /// <summary>
        /// Maps json propertys to CDM object properties
        /// </summary>
        public List<DataMappingProperty> Mapping { get; set; }

        /// <summary>
        /// Location of the Results in the json response
        /// </summary>
        public string ResultLocation { get; set; }
    }
}
