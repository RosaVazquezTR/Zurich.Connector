using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    public interface IHttpResponseService
    {

        /// <summary>
        /// Map response flag specifies whether mapping to our CDM model is required or not
        /// </summary>
        public bool MapResponse { get; set; }

        /// <summary>
        /// Apply transformation on the response and amend required CDM mapping
        /// </summary>
        /// <param name="response">
        /// Connector response
        /// </param>
        /// <param name="connectorResponse">
        /// Connector details
        /// </param>
        /// <returns>
        /// A <see cref="JToken"/>JToken response</returns>
        /// </returns>
        Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse);
        /// <summary>
        /// Apply transformation on the responseof TT passing connector ID
        /// </summary>
        /// <param name="response">
        /// Connector response
        /// </param>
        /// <param name="connectorResponse">
        /// Connector details
        /// </param>
        /// <param name="connectorId">
        /// Connector id
        /// </param>
        /// <returns>
        /// A <see cref="JToken"/>JToken response</returns>
        /// </returns>
        Task<JToken> GetJTokenResponse(string response1, ConnectorResponse response2, string id, Dictionary<string, string> requestParameter, System.Collections.Specialized.NameValueCollection query);
    }
}
