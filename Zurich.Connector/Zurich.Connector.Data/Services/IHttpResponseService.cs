using Newtonsoft.Json.Linq;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    public interface IHttpResponseService
    { 
        JToken GetJTokenResponse(string response, ConnectorResponse connectorResponse);
    }
}
