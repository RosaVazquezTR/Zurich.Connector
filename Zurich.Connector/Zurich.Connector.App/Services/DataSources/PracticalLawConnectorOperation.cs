using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Zurich.Common;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;
using Microsoft.Extensions.Configuration;

namespace Zurich.Connector.App.Services.DataSources
{
    public class PracticalLawConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;
        private readonly IConfiguration _configuration;

        public PracticalLawConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IConfiguration configuration)
        {
            _logger = logger;
            _dataMapping = dataMappingFactory.GetMapper(AuthType.OAuth2);
            _configuration = configuration;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.practicalLawConnect;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Search:
                        if (item is JObject result && result.ContainsKey("Documents"))
                        {
                            foreach (JObject doc in (result["Documents"] as JArray))
                            {
                                if (doc.ContainsKey(StructuredCDMProperties.WebUrl))
                                {
                                    doc[StructuredCDMProperties.WebUrl] = CreateUrl(doc);
                                }
                            }
                        }
                        break;
                }
            }
            catch (UriFormatException ex)
            {
                _logger.LogError("Unable to parse {entityType} web URL: {message}", entityType.ToString(), ex.Message ?? "");
            }
            return item;
        }

        /// <summary>
        /// Downloads a document to an item in its data source app
        /// </summary>
        /// <param name="itemType">The data type</param>
        /// <param name="item">The target item</param>
        /// <param name="hostName">The data source host name</param>
        /// <returns></returns>

        private string CreateUrl(JObject item)
        {
            var configuredUrl = _configuration.GetValue<string>(AppConfigKeys.PracticalLawConnectSearchHost);
            var builder = new UriBuilder("https", configuredUrl, -1);
            string url = $"{builder.Uri}{item["PlcReference"]}";

            return url;
        }
    }
}
