﻿using AutoMapper;
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

        public async Task<Dictionary<string, string>> SetParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            //TODO: changing this from a specific connector id to a parameter in the data source
            if (!connector.DataSource.InternalSorting)
                return allParameters;
            
            var thoughtFilters = JToken.Parse(allParameters["thoughtFilters"]);
            List<string> thoughtTypeIds = new();

            //Extract thoughtTypeIds from request
            foreach (var filter in thoughtFilters["filters"])
            {
                foreach(var fieldType in filter["fieldTypes"])
                    thoughtTypeIds.Add(fieldType["thoughtTypeId"].Value<string>());
            
            }
     
            //For each thoughTypeId, we add a new filter to parameters, we first need to extract the thoughFieldTypeId from onotlogies
            foreach( var thoughtTypeId in thoughtTypeIds.Distinct())
            {
                var thoughtFieldTypeId = await GetProvisionThoughtFieldTypeIdFromOntologies(thoughtTypeId);

                if (thoughtFieldTypeId == null)
                    continue;

                JObject fieldType = new()
                {
                    ["thoughtFieldTypeId"] = thoughtFieldTypeId
                };

                JObject defaultProvisionFilter = new();
                defaultProvisionFilter["fieldTypes"] = new JArray(fieldType);
                if(String.IsNullOrEmpty(allParameters["keyWord"])) {
                    defaultProvisionFilter["operator"] = "exists";
                    defaultProvisionFilter["stringValue"] = String.Empty;
                }
                else
                {
                    defaultProvisionFilter["operator"] = "contains";
                    defaultProvisionFilter["stringValue"] = allParameters["keyWord"];
                }

                

                ((JArray)thoughtFilters["filters"]).Add(defaultProvisionFilter);

            }

            allParameters["thoughtFilters"] = thoughtFilters.ToString();

            return allParameters;
        }

        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            //TODO: changing this from a specific connector id to a parameter in the data source
            if (connector.Id != "52")
                return item;

            JArray thoughtTypes = await GetThoughtTypesFromOntologies();
            
            if (item is JObject searchResult && searchResult.ContainsKey("Documents") && searchResult["Documents"].HasValues)
            {
                foreach (JObject document in searchResult["Documents"] as JArray)
                {
                    var thoughtType = thoughtTypes.Where(thoughtType =>
                        thoughtType["id"].Value<string>() == document["AdditionalProperties"]["thoughtTypeId"].Value<string>()).FirstOrDefault();

                    var thoughtFieldType = thoughtType?["fieldTypes"].Where(fieldType => 
                        fieldType["id"].Value<string>() == document["AdditionalProperties"]["thoughtFieldTypeId"].Value<string>()).FirstOrDefault();

                    document["AdditionalProperties"]["thoughtTypeName"] = thoughtType?["name"].Value<string>();
                    document["AdditionalProperties"]["thoughtFieldTypeName"] = thoughtFieldType?["name"].Value<string>();
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

        /// <summary>
        /// Gets the "Provision" thought id field given a thoughtTypeId information from ontologies
        /// </summary>
        public async Task<string> GetProvisionThoughtFieldTypeIdFromOntologies(string thoughtTypeId)
        {
            JArray thoughtTypes = await GetThoughtTypesFromOntologies();

            var thoughtType = thoughtTypes.Where(thoughtType => thoughtType["id"].Value<string>() == thoughtTypeId).FirstOrDefault();
            var provisionThought = thoughtType?["fieldTypes"].Where(fieldType => fieldType["name"].Value<string>() == "Provision").FirstOrDefault();

            return provisionThought?["id"].Value<string>();
        }
    }
}
