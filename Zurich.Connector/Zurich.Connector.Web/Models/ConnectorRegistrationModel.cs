using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zurich.Connector.Web.Models
{
    public class ConnectorRegistrationModel
    {
        /// <summary>
        /// connectorid        
        /// </summary>
        public string ConnectorId { get; set; }

        /// <summary>
        /// data source ID
        /// </summary>
        public string DataSourceId { get; set; }
    }
}
