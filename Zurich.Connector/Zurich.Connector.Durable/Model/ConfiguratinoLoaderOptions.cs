using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;

namespace Zurich.Connector.Durable.Model
{
    /// <summary>
    /// To store configuration options during startup.
    /// </summary>
    public class ConfiguratinoLoaderOptions
    {
        /// <summary>
        /// Audience
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// Issuer
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// OAuth DB Connection string
        /// </summary>
        public string OAuthDbConnString { get; set; }
        /// <summary>
        /// Product Connection String
        /// </summary>
        public string ProductConnectionString { get; set; }
        /// <summary>
        /// OAuth Options
        /// </summary>
        public OAuthOptions OAuthOptions { get; set; }
        /// <summary>
        /// Azure AD options
        /// </summary>
        public AzureAdOptions AzureAdOptions { get; set; }
        /// <summary>
        /// Configuration Refresher
        /// </summary>
        public IConfigurationRefresher ConfigurationRefresher { get; set; }
        /// <summary>
        /// Micro service options
        /// </summary>
        public MicroServiceOptions MicroServOptions { get; set; }
        /// <summary>
        /// Table storage options
        /// </summary>
        public StatsStorageContainerOptions TableStorageOptions { get; set; }
        /// <summary>
        /// Configuration providers
        /// </summary>
        public IEnumerable<IConfigurationProvider> ConfigurationProviders { get; set; }
        /// <summary>
        /// Cosmos DB Options
        /// </summary>
        public CosmosDbOptions ConnectorCosmosDbOptions { get; set; }
        /// <summary>
        /// Connector cosmos client options.
        /// </summary>
        public CosmosClientSettings ConnectorCosmosClientOptions { get; set; }
    }
}
