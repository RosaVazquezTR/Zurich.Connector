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

        /// <summary>
        /// Partition information for the cosmos record.
        /// TODO: update casing after updating cosmos
        /// </summary>
        public string partitionkey { get; set; }

        /// <summary>
        /// connectorid        
        /// </summary>
        public string ConnectorId { get; set; }
        /// <summary>
        /// userID        
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// tenantid ID
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Use For App Name... TODO: Is this needed?
        /// </summary>
        public string AppName { get; set; }
    }

    /// <summary>
    /// Reprsent's deiffrent RegistrationStatus
    /// </summary>
    public enum Status
    {
        registered = 1,
        notRegister = 2
    }
}
