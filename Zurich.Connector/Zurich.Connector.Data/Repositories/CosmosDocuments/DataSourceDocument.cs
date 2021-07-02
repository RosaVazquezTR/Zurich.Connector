using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Cosmos;
using Zurich.Common.Services.Security;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    /// <summary>
    /// Store definitions for different data sources, mainly authorization details. 
    /// </summary>
    public class DataSourceDocument : CosmosDocument
    {
        public DataSourceDocument() : base(CosmosConstants.DataSourcePartitionKey) { }
        /// <summary>
        /// Data source ID
        /// </summary>
        public override string Id { get; set; }
        public string partitionkey { get { return CosmosConstants.DataSourcePartitionKey; } }
        /// <summary>
        /// Data source name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Data source description
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Data source infomation url.
        /// </summary>
        public string infoUrl { get; set; }
        /// <summary>
        /// Extra request context. Ex: "iManageCustomerId"
        /// </summary>
        public string extraRequestContext { get; set; }
        /// <summary>
        /// Define security definition
        /// </summary>
        public SecurityDefinition securityDefinition { get; set; }
        /// <summary>
        /// Define scope
        /// </summary>
        public DataSourceScopes scopes { get; set; }
        /// <summary>
        /// App Code
        /// </summary>
        public string appCode { get; set; }

        /// <summary>
        /// Application Type.
        /// </summary>
        public OAuthApplicationType? appType { get; set; }

        /// <summary>
        /// Locale (ex: US / UK etc).
        /// </summary>
        public string locale { get; set; }
    }

    /// <summary>
    /// Store security definition 
    /// </summary>
    public class SecurityDefinition
    {
        /// <summary>
        /// Type. Ex: "oauth2"
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Flow. Ex: "accessCode"
        /// </summary>
        public string flow { get; set; }
        /// <summary>
        /// Default security definition
        /// users can override default values, if left blank then needs to be requested during connector registration
        /// </summary>
        public SecurityDefinitionDetails defaultSecurityDefinition { get; set; }

    }

    /// <summary>
    /// Store data source scopes.
    /// </summary>
    public class DataSourceScopes
    {

    }

    /// <summary>
    /// Store security definition details
    /// </summary>
    public class SecurityDefinitionDetails
    {
        /// <summary>
        /// Authorization URL
        /// </summary>
        public string authorizationURL { get; set; }
        /// <summary>
        /// Authorization Path
        /// </summary>
        public string authorizationPath { get; set; }
        /// <summary>
        /// Token URL
        /// </summary>
        public string tokenURL { get; set; }
        /// <summary>
        /// Token Path
        /// </summary>
        public string tokenPath { get; set; }
        /// <summary>
        /// ClientId of the key vault
        /// </summary>
        public string keyVaultClientId { get; set; }
        /// <summary>
        /// Secret of the key vault.
        /// </summary>
        public string keyVaultSecret { get; set; }
        /// <summary>
        /// Authorization Header
        /// </summary>
        public string authorizationHeader { get; set; }
        /// <summary>
        /// Grant Type. Ex: client_credentials
        /// </summary>
        public string grantType { get; set; }
        /// <summary>
        /// sendCredentialsInBody. Ex: true / false.
        /// </summary>
        public bool? sendCredentialsInBody { get; set; }

    }
}
