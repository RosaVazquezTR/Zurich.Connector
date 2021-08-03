using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <summary>
        /// Define security definition
        /// </summary>
        public SecurityDefinitionModel SecurityDefinition { get; set; }
        // TODO
        // Define scope here.
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
}
