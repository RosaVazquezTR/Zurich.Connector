using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Web.Models
{
    public class ConnectorConfigViewModel
    {
        /// <summary>
        /// The connection id that should be used for the endpoint
        /// </summary>
        public string Id { get; set; }

        /// <summary>
		/// The code used for the application we are calling
		/// </summary>
        public string AppCode { get; set; }

        /// <summary>
		/// The type of authorization used in this endpoint
		/// </summary>
        public DataMappingAuth AuthType { get; set; }

        /// <summary>
        /// The DataType of a given request
        /// </summary>
        public string[] filters { get; set; }

        /// <summary>
        /// Information about how to call the api
        /// </summary>
        public DataMappingApiRequest Api { get; set; }

        /// <summary>
        /// Maps json propertys to CDM object properties
        /// </summary>
        public List<DataMappingProperty> CDMData { get; set; }
    }
}
