using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zurich.Connector.Web.Models
{
    public class DataSourceViewModel
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
        public SecurityDefinitionViewModel SecurityDefinition { get; set; }
        // TODO
        // Define scope here.
    }

    /// <summary>
    /// Store security definition 
    /// </summary>
    public class SecurityDefinitionViewModel
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
        public SecurityDefinitionDetailsViewModel DefaultSecurityDefinition { get; set; }

    }

    /// <summary>
    /// Store security definition details
    /// </summary>
    public class SecurityDefinitionDetailsViewModel
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

    }
}
