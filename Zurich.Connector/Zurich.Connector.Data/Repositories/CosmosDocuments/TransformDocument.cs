using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Cosmos;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    public class TransformDocument : CosmosDocument
    {
        public TransformDocument() : base(CosmosConstants.TransformPartitionKey) { }
        /// <summary>
        /// Transform Id
        /// </summary>
        public override string Id { get; set; }
        public string partitionkey { get { return CosmosConstants.TransformPartitionKey; } }
        /// <summary>
        /// DataSource Id
        /// </summary>
        public string dataSourceId { get; set; }
        /// <summary>
        /// Transform type
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Transform details
        /// </summary>
        public string transform { get; set; }
    }
}
