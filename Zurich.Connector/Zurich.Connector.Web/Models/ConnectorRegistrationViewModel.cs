using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Web.Enum;
namespace Zurich.Connector.Web.Models
{
    public class ConnectorRegistrationViewModel
    {
        /// <summary>
        /// use for Connectorid        
        /// </summary>

        public string  Connectorid { get; set; }
        /// <summary>
        /// UserID        
        /// </summary>
        public string Userid { get; set; }
        /// <summary>
        /// tenantid ID
        /// </summary>
        public string Tenantid { get; set; }

        /// <summary>
        /// Use For appname
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// use for registrationStatus
        /// </summary>
        public RegistrationStatus RegistrationStatus { get; set; }
    }
}
