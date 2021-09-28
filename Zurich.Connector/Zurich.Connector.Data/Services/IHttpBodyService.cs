using System.Collections.Specialized;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    public interface IHttpBodyService
    {
        string CreateBody(ConnectorDocument dataTypeInformation, NameValueCollection query);
    }
}
