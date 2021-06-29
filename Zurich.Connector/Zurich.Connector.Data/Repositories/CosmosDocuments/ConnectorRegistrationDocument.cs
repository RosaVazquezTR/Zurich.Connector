using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Cosmos;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    public class ConnectorRegistrationDocument : CosmosDocument
    {
        public ConnectorRegistrationDocument() : base(CosmosConstants.ConnectorRegistrationPartitionKey) { }
        public string partitionkey { get { return CosmosConstants.ConnectorRegistrationPartitionKey; } }

        /// <summary>
        /// connectorid        
        /// </summary>
        public string connectorid { get; set; }
        /// <summary>
        /// userID        
        /// </summary>
        public string userid { get; set; }

        /// <summary>
        /// tenantid ID
        /// </summary>
        public string tenantid { get; set; }

        /// <summary>
        /// data source ID
        /// </summary>
        public string datasourceid { get; set; }

       
        /// <summary>
        /// Use For appname
        /// </summary>
        public string appname { get; set; }

        /// <summary>
        /// use for registrationStatus
        /// </summary>
        public Status registrationStatus { get; set; }
    }

    /// <summary>
    /// Reprsent's deiffrent RegistrationStatus
    /// </summary>
    public enum Status
    {
        register = 1,
        notRegister = 2
    }
}
