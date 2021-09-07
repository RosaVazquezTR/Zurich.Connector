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
        /// Unique Id for Registration
        /// </summary>
        public string Id { get; set; }

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
        /// Use For appname
        /// </summary>
        public string AppName { get; set; }
    }
}
