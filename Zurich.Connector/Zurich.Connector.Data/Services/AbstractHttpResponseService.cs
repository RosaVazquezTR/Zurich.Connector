using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    public abstract class AbstractHttpResponseService
	{
		public virtual bool MapResponse { get; set; }
		public async virtual Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse)
		{
			return JToken.Parse(response);
		}
	}
}
