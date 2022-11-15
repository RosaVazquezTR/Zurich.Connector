using System.Text.Json.Serialization;
using Zurich.Common.Cosmos;
using Zurich.Common.Models.Connectors;
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
        /// Define registration Information
        /// </summary>
        public RegistrationInfo registrationInfo { get; set; }

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
        /// The domain for the data sorce
        /// TODO: eventually this will come from oAuthService
        /// </summary>
        public string domain { get; set; }

        /// <summary>
        /// Application Type.
        /// </summary>
        public OAuthApplicationType? appType { get; set; }

        /// <summary>
        /// Locale (ex: US / UK etc).
        /// </summary>
        public string locale { get; set; }

        /// <summary>
        /// TODO: Remove this, needed for a work around at the moment
        /// </summary>
        public string productType { get; set; }
        /// <summary>
        /// Whether or not the datasource contains multiple instances
        /// </summary>
        public bool CombinedLocations { get; set; }

        /// <summary>
        /// Whether or not the connector response needs to be sorted on our side
        /// </summary>
        public bool InternalSorting { get; set; }
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

    public class RegistrationInfo
    {
        /// <summary>
        /// Flag for registrationMode. ex:"Automatic"
        /// </summary>
        public RegistrationEntityMode registrationMode { get; set; }

        /// <summary>
        /// domain Required ex: true or false
        /// </summary>
        public bool domainRequired { get; set; }

        /// <summary>
        /// client And Secret Required boolean value ex: true or false.
        /// </summary>
        public bool clientAndSecretRequired { get; set; }

        /// <summary>
        /// connection Message
        /// </summary>
        public string connectionMessage { get; set; }

        /// <summary>
        /// registration File Required 
        /// </summary>
        public bool registrationFileRequired { get; set; }


        /// <summary>
        /// domain Specific Information
        /// </summary>
        public DomainSpecificInformation domainSpecificInformation { get; set; }


    }

    public class DomainSpecificInformation
    {
        /// <summary>
        /// Client And Secret Required 
        /// </summary>
        public bool clientAndSecretRequired { get; set; }

        /// <summary>
        /// connection Message
        /// </summary>
        public string connectionMessage { get; set; }

        /// <summary>
        /// Registration File Required
        /// </summary>
        public bool registrationFileRequired { get; set; }

    }

}
