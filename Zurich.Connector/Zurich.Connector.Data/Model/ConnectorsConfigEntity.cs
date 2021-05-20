using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    public class ConnectorsConfigEntity
    {
        /// <summary>
        /// The connection id for the endpoint
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The app code that should be used for the product
        /// </summary>
        public string AppCode { get; set; }
        /// <summary>
        /// The Auth information that will be used for the app
        /// </summary>
        public DataMappingAuth Auth { get; set; }
        /// <summary>
        /// Types of data the apps support and the corresponding mapping
        /// </summary>
        public DataMappingEndpoint Endpoint { get; set; }
        /// <summary>
        /// Represents the redirection URL
        /// </summary>
        public string redirectionURL { get; set; }

        /// <summary>
        /// Represents the filters
        /// </summary>
        public string[] filters { get; set; }
    }


    public class ConnectorsEntity
    {
        /// <summary>
        /// List of Connectors that are supported
        /// </summary>
        public List<ConnectorsConfigEntity> connectors { get; set; }
    }

}
