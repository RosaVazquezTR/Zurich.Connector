using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
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
        Task<T> GetAndMapResults<T>(ConnectorDocument dataTypeInformation, string transferToken = null, NameValueCollection query = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Retrieve token based on appCode, appType, locale, grandType
        /// </summary>
        /// <param name="appCode">Application code to get the data from</param>
        /// <returns>Token to use</returns>
        Task<AppToken> RetrieveToken(string appCode, OAuthApplicationType? appType = null,
                                     string locale = null, string grandType = null, bool? sendCredentialsInBody = false, string productType = null);

        /// <summary>
        /// Method will map a string json object to the CDM object passed in
        /// </summary>
        /// <typeparam name="T">The CommonDataModel that should be returned</typeparam>
        /// <param name="jsonResponse">the Jtoken object of the response</param>
        /// <param name="resultLocation">What values we should grab to get to the data ie: "data.results"</param>
        /// <param name="propertyMap">Connector Document object</param>
        /// <returns>The CDM Model</returns>
        Task<T> MapToCDM<T>(JToken jsonResponse, string resultLocation, ConnectorDocument propertyMap);
    }
}
