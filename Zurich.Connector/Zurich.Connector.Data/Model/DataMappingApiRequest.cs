using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// Used to hold informationa bout a given request
    /// </summary>
    public class DataMappingApiRequest
    {
        /// <summary>
        /// HTTP Method type of request
        /// </summary>
        public string MethodType { get; set; }

        /// <summary>
        /// URL path of a request
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Header that should be used for a request
        /// </summary>
        public string AuthHeader { get; set; }
    }
}
