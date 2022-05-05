using Newtonsoft.Json.Linq;
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
    }
}
