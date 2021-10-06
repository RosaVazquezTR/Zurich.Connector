using System.Collections.Specialized;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    public abstract class AbstractHttpBodyService
	{

        public virtual string CreateBody(ConnectorDocument dataTypeInformation, NameValueCollection query)
		{
			return string.Empty;
		}
	}
}
