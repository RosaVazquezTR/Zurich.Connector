using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.DataMap
{
    /// <summary>
    /// Data mapping Interface
    /// </summary>
    public interface IDataMapping
    {
        /// <summary>
        /// Gets data based on app code and DataType
        /// </summary>
        /// <typeparam name="T">The CommonDataModel that should be returned</typeparam>
        /// <param name="appCode">Application code to get the data from</param>
        /// <param name="dataType">Data type to retrieve</param>
        /// <param name="query">Query parameters mapped from cosmo DB</param>
        /// <returns>The T model based on the request or default(T)</returns>
        Task<T> Get<T>(ConnectorDocument dataTypeInformation, string transferToken = null, NameValueCollection query = null);

        /// <summary>
        /// Retrieve token based on appCode, appType, locale, grandType
        /// </summary>
        /// <param name="appCode">Application code to get the data from</param>
        /// <returns>Token to use</returns>
        Task<AppToken> RetrieveToken(string appCode, OAuthApplicationType? appType = null,
                                     string locale = null, string grandType = null, bool? sendCredentialsInBody = false);

        /// <summary>
        /// Retreives the Map to be used for this product call
        /// </summary>
        /// <param name="appCode">Application code to get the data from</param>
        /// <param name="hostname">The domain of the api being called</param>
        /// <returns>Returns Connector Document</returns>
        Task<ConnectorDocument> RetrieveProductInformationMap(string connectionId, string hostname);

        /// <summary>
        /// This will update the url to have correct values replaced if dataMap is setup correctly
        /// Note: we might want to require this information to be passed in/do this differently
        /// </summary>
        /// <param name="appCode">Application code to get the data from</param>
        /// <param name="urlPath">Current url path to be used</param>
        /// <returns>The urlPath without formating</returns>
        Task<string> UpdateUrl(string urlPath, ConnectorDocument dataTypeInformation, string transferToken = null);

        /// <summary>
        /// Method will map a string json object to the CDM object passed in
        /// </summary>
        /// <typeparam name="T">The CommonDataModel that should be returned</typeparam>
        /// <param name="stringJsonResponse">the string value of the json response coming in</param>
        /// <param name="resultLocation">What values we should grab to get to the data ie: "data.results"</param>
        /// <param name="propertyMap">Connector Document object</param>
        /// <returns>The CDM Model</returns>
        Task<T> MapToCDM<T>(string stringJsonResponse, string resultLocation, ConnectorDocument propertyMap);
    }

    public class DataMapping : DataMappingBase, IDataMapping
    {
        public DataMapping(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMapping> logger)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
        }
    }
}
