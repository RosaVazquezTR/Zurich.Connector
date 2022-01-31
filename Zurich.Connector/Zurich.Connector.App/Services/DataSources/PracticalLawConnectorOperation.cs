using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    public class PracticalLawConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;
        private readonly IConfiguration _configuration;
        private List<string> _supportedPracticalLawAppCodes = new List<string>() { "PracticalLawConnect", "PLCUS", "PLCUK", "PLCCA", "CBTPRACPT", "PracticalLawConnect-Search", "PLCUS-Search", "PLCUK-Search"
                                                                                   , "PLCCA-Search", "CBTPRACPT-Search" };

        public PracticalLawConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IConfiguration configuration)
        {
            _logger = logger;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _configuration = configuration;
        }

        public bool IsCompatible(string appCode)
        {
            return _supportedPracticalLawAppCodes.Contains(appCode);
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Search:
                        if (item is JObject result && result.ContainsKey("Documents") && result["Documents"].HasValues)
                        {
                            foreach (JObject doc in (result["Documents"] as JArray))
                            {
                                if (doc.ContainsKey(StructuredCDMProperties.WebUrl))
                                {
                                    doc[StructuredCDMProperties.WebUrl] = CreateUrl(doc, appCode);
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

        private string CreateUrl(JObject item, string appCode)
        {
            string configuredUrl = "";
           
            if (appCode == KnownDataSources.practicalLawConnect || appCode == KnownDataSources.practicalLawConnectSearch || appCode == KnownDataSources.practicalLawConnectOnePass || appCode == KnownDataSources.practicalLawConnectSearchOnePass)
                configuredUrl = _configuration.GetValue<string>(AppConfigKeys.PracticalLawConnectSearchHost);
            else if (appCode == KnownDataSources.plcUS || appCode == KnownDataSources.plcUSSearch)
                configuredUrl = _configuration.GetValue<string>(AppConfigKeys.PracticalLawUSHost);
            else if (appCode == KnownDataSources.plcUK || appCode == KnownDataSources.plcUKSearch)
                configuredUrl = _configuration.GetValue<string>(AppConfigKeys.PracticalLawUKHost);
            else if (appCode == KnownDataSources.plcCA || appCode == KnownDataSources.plcCASearch)
                configuredUrl = _configuration.GetValue<string>(AppConfigKeys.PracticalLawCAHost);

            if (!string.IsNullOrEmpty(configuredUrl))
            {
                var builder = new UriBuilder("https", configuredUrl, -1);
                return $"{builder.Uri}{item[StructuredCDMProperties.AdditionalProperties]["plcReference"]}";
            }
            else
            return "";
        }
    }
}
