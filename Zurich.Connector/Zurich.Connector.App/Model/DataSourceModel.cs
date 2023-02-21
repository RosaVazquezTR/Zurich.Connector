using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.Connectors;
using Zurich.Common.Services.Security;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Store data source details.
    /// </summary>
    public class DataSourceModel
    {
        /// <summary>
        /// Data source ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Data source name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Data source description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// App code
        /// </summary>
        public string AppCode { get; set; }
        /// <summary>
        /// The domain of the datasource for API calls
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// Application Type.
        /// </summary>
        public OAuthApplicationType? appType { get; set; }
        /// <summary>
        /// Requires New Refresh Token
        /// </summary>
        public bool RequiresNewToken { get; set; }
        /// <summary>
        /// Locale (ex: US / UK etc).
        /// </summary>
        public string locale { get; set; }
        /// <summary>
        /// Data source infomation url.
        /// </summary>
        public string InfoUrl { get; set; }
        /// <summary>
        /// Extra request context. Ex: "iManageCustomerId"
        /// </summary>
        public string ExtraRequestContext { get; set; }
        /// Flag for registrationMode. ex:"Automatic"
        /// </summary>

        /// <summary>
        /// Define registration Information
        /// </summary>
        public RegistrationInfoModel RegistrationInfo { get; set; }
        /// <summary>
        /// Define security definition
        /// </summary>
        public SecurityDefinitionModel SecurityDefinition { get; set; }
        /// <summary>
        /// Locale (ex: US / UK etc).
        /// </summary>
        public string Locale { get; set; }
        
        /// <summary>
        /// TODO: Remove this, needed for a work around at the moment
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// Whether or not the datasource contains multiple instances
        /// </summary>
        public bool CombinedLocations { get; set; }

        /// <summary>
        /// For connectors that do not support offset
        /// </summary>
        public bool ManualOffset { get; set; }
    }

    /// <summary>
    /// Store security definition 
    /// </summary>
    public class SecurityDefinitionModel
    {
        /// <summary>
        /// Type. Ex: "oauth2"
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Flow. Ex: "accessCode"
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// Default security definition
        /// users can override default values, if left blank then needs to be requested during connector registration
        /// </summary>
        public SecurityDefinitionDetailsModel DefaultSecurityDefinition { get; set; }

    }

    /// <summary>
    /// Store security definition details
    /// </summary>
    public class SecurityDefinitionDetailsModel
    {
        /// <summary>
        /// Authorization URL
        /// </summary>
        public string AuthorizationURL { get; set; }
        /// <summary>
        /// Authorization Path
        /// </summary>
        public string AuthorizationPath { get; set; }
        /// <summary>
        /// Authorization Header
        /// </summary>
        public string AuthorizationHeader { get; set; }
        /// <summary>
        /// Token URL
        /// </summary>
        public string TokenURL { get; set; }
        /// <summary>
        /// Token Path
        /// </summary>
        public string TokenPath { get; set; }
        /// <summary>
        /// ClientId of the key vault
        /// </summary>
        public string KeyVaultClientId { get; set; }
        /// <summary>
        /// Secret of the key vault.
        /// </summary>
        public string KeyVaultSecret { get; set; }
        /// <summary>
        /// Grant Type. Ex: client_credentials
        /// </summary>
        public string grantType { get; set; }
        /// <summary>
        /// sendCredentialsInBody. Ex: true / false.
        /// </summary>
        public bool? sendCredentialsInBody { get; set; }
    }
    public class RegistrationInfoModel
    {
        /// <summary>
        /// Flag for registrationMode. ex:"Automatic"
        /// </summary>
        public RegistrationEntityMode RegistrationMode { get; set; }

        /// <summary>
        /// Domain Required ex: true or false
        /// </summary>
        public bool DomainRequired { get; set; }

        /// <summary>
        /// client And Secret Required boolean value ex: true or false.
        /// </summary>
        public bool ClientAndSecretRequired { get; set; }

        /// <summary>
        /// connection Message
        /// </summary>
        public string ConnectionMessage { get; set; }

        /// <summary>
        /// registration File Required 
        /// </summary>
        public bool RegistrationFileRequired { get; set; }

        /// <summary>
        /// domain Specific Information
        /// </summary>
        public DomainSpecificInformationModel DomainSpecificInformation { get; set; }


    }

    public class DomainSpecificInformationModel
    {
        /// <summary>
        /// Client And Secret Required 
        /// </summary>
        public bool ClientAndSecretRequired { get; set; }

        /// <summary>
        /// connection Message
        /// </summary>
        public string ConnectionMessage { get; set; }

        /// <summary>
        /// Registration File Required
        /// </summary>
        public bool RegistrationFileRequired { get; set; }

    }
}
