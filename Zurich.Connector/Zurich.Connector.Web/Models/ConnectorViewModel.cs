using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// Class with only connection information shown to user
    /// </summary>
    public class ConnectorViewModel
    {
        /// <summary>
        /// The connector id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The type of connector
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Data source for connector. Maps to app code for now
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Type of authorization protocol for the connector
        /// </summary>
        public string AuthType { get; set; }

        /// <summary>
        /// Time of creation
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// Domain of the api endpoint
        /// </summary>
        public string? Domain { get; set; }
    }
}
