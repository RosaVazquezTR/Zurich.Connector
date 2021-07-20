using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Enum;

namespace Zurich.Connector.App.Model
{
    public class ConnectorRegistration
    {

        /// <summary>
        /// use for ConnectorId
        /// </summary>
        public string ConnectorId { get; set; }
        /// <summary>
        /// UserID        
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// tenantid ID
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Data source ID
        /// </summary>
        public string DataSourceId { get; set; }

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
