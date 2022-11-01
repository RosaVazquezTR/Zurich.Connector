using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.App.Services.DataSources
{
    public class ThoughtTraceOntologiesConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ICosmosService _cosmosService;
        private readonly IDataMapping _dataMapping;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ThoughtTraceOntologiesConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IMapper mapper, ICosmosService cosmosService)
        {
            _logger = logger;
            _mapper = mapper;
            _cosmosService = cosmosService;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.thoughtTrace;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            return item;
        }

        public async Task<Dictionary<string, string>> SetSortParameters(Dictionary<string, string> allParameters)
        {
            return allParameters;
        }

        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            JArray thoughtTypes = await GetThoughtTypesFromOntologies();
            
            if (item is JObject searchResult && searchResult.ContainsKey("Documents") && searchResult["Documents"].HasValues)
            {
                foreach (JObject document in searchResult["Documents"] as JArray)
                {
                    var thoughtType = thoughtTypes.Where(thoughtType =>
                        thoughtType["id"].Value<string>() == document["AdditionalProperties"]["thoughtTypeId"].Value<string>()).FirstOrDefault();

                    var thoughtFieldType = thoughtType["fieldTypes"].Where(fieldType => 
                        fieldType["id"].Value<string>() == document["AdditionalProperties"]["thoughtFieldTypeId"].Value<string>()).FirstOrDefault();

                    document["AdditionalProperties"]["thoughtTypeName"] = thoughtType["name"].Value<string>();
                    document["AdditionalProperties"]["thoughtFieldTypeName"] = thoughtFieldType["name"].Value<string>();
                }
            }

            return item;
        }

        /// <summary>
        /// Gets the ontologies information from ThoughtTrace
        /// </summary>
        private async Task<JArray> GetThoughtTypesFromOntologies()
        {
            ConnectorModel connectorModel = await _cosmosService.GetConnector("60", true);            
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

            JToken ontologiesResponse = await _dataMapping.GetAndMapResults<JToken>(connectorDocument, string.Empty, null, null, null);

            JArray thoughtTypes = new JArray();

            // As we don't need ontology metadata, I put together all tougtTypes to make search easier
            foreach (JToken ontology in ontologiesResponse)
            {
                foreach(JToken thoughtType in ontology["thoughtTypes"])
                    thoughtTypes.Add(thoughtType);
            }

            return thoughtTypes;
        }
    }
}
