using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Connector.App.Model;
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
        private readonly Dictionary<string, List<String>> _supportedFiltersByAppCode = new(){
               {"PLCUK", new List<string>{ "practiceAreaRefList", "resourceTypeRefList", "jurisdictionRefList"} }
            };
        private List<string> _supportedPracticalLawAppCodes = new List<string>() { "PracticalLawConnect", "PLCUS", "PLCUK", "PLCCA", "PLCAU", "CBTPRACPT", "PracticalLawConnect-Search", "PLCUS-Search", "PLCUK-Search"
                                                                                   , "PLCCA-Search", "PLCAU-Search", "CBTPRACPT-Search" };

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
                                if(doc.ContainsKey(StructuredCDMProperties.Type) && doc[StructuredCDMProperties.Type].Type.ToString() is "Array")
                                {
                                    var docType = doc.SelectToken(StructuredCDMProperties.Type)?.ToObject<String []>();
                                    doc[StructuredCDMProperties.Type] = docType.FirstOrDefault();
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

        public async Task<Dictionary<string, string>> SetParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            return allParameters;
        }
        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            // Only enable for PLCUK at the moment
            if (connector.DataSource.AppCode != KnownDataSources.plcUK)
                return item;

            var facetToCountDictionary = new Dictionary<string, int>();
            var supportedFilters = _supportedFiltersByAppCode[connector.DataSource.AppCode];
            JArray facets = new();


            if (item is JObject searchResult && searchResult.ContainsKey("Documents") && searchResult["Documents"].HasValues)
            {
               
                // Put together in a single IEnumerable all found filters id found in the documents
                var allFacetsResponse = (searchResult["Documents"] as JArray)
                    .Select(document => supportedFilters.SelectMany(f => document["AdditionalProperties"][f].HasValues ? document["AdditionalProperties"][f].Values<string>() : Enumerable.Empty<string>() )).SelectMany(x => x);

                // Group and count repeated filters found
                var groups = allFacetsResponse.GroupBy(f => f).Select(facet => 
                new {
                    Facet = facet.Key,
                    Count = facet.Count()
                });

                facetToCountDictionary = groups.ToDictionary(g => g.Facet, g => g.Count);

                foreach (var filterModel in connector.Filters)
                {
                    JArray facetItemList = new();

                    filterModel.FilterList.ForEach(filterValue =>
                    {
                        if (facetToCountDictionary.TryGetValue(filterValue.Id, out int itemCount))
                        {
                            JObject facetItem = new()
                            {
                                ["Id"] = filterValue.Id,
                                ["Count"] = itemCount
                            };

                            facetItemList.Add(facetItem);
                        }
                    });

                    if(facetItemList.Count != 0)
                    {

                        JObject facetObject = new()
                        {
                            ["facetParameter"] = filterModel.RequestParameter,
                            ["facetList"] = facetItemList
                        };

                        facets.Add(facetObject);
                    }
                }
            }

           
            ((JObject)item).TryAdd("facets", facets);

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
            else if (appCode == KnownDataSources.plcAU || appCode == KnownDataSources.plcAUSearch)
                configuredUrl = _configuration.GetValue<string>(AppSettings.PracticalLawAUHost);

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
