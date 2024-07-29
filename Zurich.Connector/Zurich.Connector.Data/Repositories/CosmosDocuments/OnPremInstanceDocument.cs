using Newtonsoft.Json;
using System;
using Zurich.Common.Cosmos;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    public class OnPremInstanceDocument : CosmosDocument
    {
        public OnPremInstanceDocument(string type) : base(type)
        {
        }

        /// <summary>
        /// Gets or sets the application code
        /// </summary>
        public string AppCode { get; set; }

        /// <summary>
        /// Gets or sets base url
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets client secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets client id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets created user id
        /// </summary>
        public string CreatedUserId { get; set; }

        /// <summary>
        /// Gets or sets last modified user id
        /// </summary>
        public string LastModifiedUserId { get; set; }

        /// <summary>
        /// Gets or sets the partition key (connector id)
        /// </summary>
        [JsonProperty("partitionkey")]
        public string PartitionKey { get => CosmosConstants.OnPremPartitionKey; }

        /// <summary>
        /// Gets or sets the row key (custom connector id)
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
