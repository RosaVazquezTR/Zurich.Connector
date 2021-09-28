using Newtonsoft.Json.Linq;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    public abstract class AbstractHttpResponseService
	{
        public virtual JToken GetJTokenResponse(string response, ConnectorResponse connectorResponse)
		{
			return JToken.Parse(response);
		}
	}
}
