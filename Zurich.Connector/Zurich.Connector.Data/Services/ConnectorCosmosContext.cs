using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Repositories.Cosmos;

namespace Zurich.Connector.Data.Services
{
    public class ConnectorCosmosContext : CosmosContext
    {
        public ConnectorCosmosContext(ICosmosClientFactory documentClientFactory, CosmosDbOptions cosmosOptions) : base(documentClientFactory, cosmosOptions)
        {

        }
    }
}
