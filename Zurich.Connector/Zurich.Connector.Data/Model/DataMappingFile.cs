using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// The Main file object for JsonMapping
    /// </summary>
    public class DataMappingFile
    {
        /// <summary>
        /// List of apps that are supported
        /// </summary>
        public List<DataMappingConnection> Connections { get; set; }
    }

    public class DataMappingConnection
    {
        /// <summary>
        /// The connection id for the endpoint
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The type of connector
        /// </summary>
        public ConnectorEntityType EntityType { get; set; }
        /// <summary>
        /// The app code that should be used for the product
        /// </summary>
        public string AppCode { get; set; }
        /// <summary>
        /// The Auth information that will be used for the app
        /// </summary>
        public DataMappingAuth Auth { get; set; }
        /// <summary>
        /// Types of data the apps support and the corresponding mapping
        /// </summary>
        public DataMappingEndpoint Endpoint { get; set; }
    }

    /// <summary>
    /// Auth information that will be used to get information in the app.
    /// </summary>
    public class DataMappingAuth
    {
        /// <summary>
        /// Type of authorization the endpoint should use
        /// </summary>
        public AuthType Type { get; set; }
        /// <summary>
        /// OAuth information that will be used during the OAuth process
        /// </summary>
        public DataMappingOAuthType OAuth { get; set; }

    }

    /// <summary>
    /// Information needed to use the OAuth type
    /// </summary>
    public class DataMappingOAuthType
    {
        /// <summary>
        /// Auth url that will be used during the OAuth process
        /// </summary>
        public string AuthUrl { get; set; }

        /// <summary>
        /// Token Url that will be used during the OAuth process
        /// </summary>
        public string TokenUrl { get; set; }

        /// <summary>
        /// The Authentication header that should be used when making a request to an app
        /// </summary>
        public string AuthHeader { get; set; }
    }


    /// <summary>
    /// Holds information about app apis and property mappings
    /// </summary>
    public class DataMappingEndpoint
    {
        /// <summary>
        /// The DataType of the api
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Information around the api call to make
        /// </summary>
        public DataMappingApi Api { get; set; }
        /// <summary>
        /// Where the results are in the json structure
        /// </summary>
        public string ResultLocation { get; set; }
        /// <summary>
        /// List of properties and the map to the CDM model
        /// </summary>
        public List<DataMappingProperty> Mapping { get; set; }
    }

    /// <summary>
    /// Basically a key value pair for CDM property to API Property
    /// </summary>
    public class DataMappingProperty
    {
        /// <summary>
        /// Property that will be set for the CDM object
        /// </summary>
        public string CDMProperty { get; set; }

        /// <summary>
        /// Json property that will be returned from the API call
        /// </summary>
        public string APIProperty { get; set; }
    }

    /// <summary>
    /// Information around the api endpoint
    /// </summary>
    public class DataMappingApi
    {
        /// <summary>
        /// The endpoint path, can include some other information like calls elsewhere ie)/work/api/v2/customers/{UserInfo.customer_id}/documents"
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// API request type
        /// </summary>
        public string MethodType { get; set; }
    }
}
