using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    public class WestLawIEConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;

        public WestLawIEConnectorOperation(ILogger<IConnectorDataSourceOperations> logger)
        {
            _logger = logger;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.westLawIE;
        }

        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            item.AdditionalProperties = new JObject();

            return item;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            return item;
        }

        public async Task<Dictionary<string, string>> SetParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            return allParameters;
        }
    }
}
