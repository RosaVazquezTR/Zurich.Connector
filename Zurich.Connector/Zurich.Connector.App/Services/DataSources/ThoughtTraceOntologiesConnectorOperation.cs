using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThoughtTraceOntologiesConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IMapper mapper, ICosmosService cosmosService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _cosmosService = cosmosService;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _httpContextAccessor = httpContextAccessor;
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
            

            allParameters = TTMap(allParameters);

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
                        thoughtType["id"].Value<string>() == document["AdditionalProperties"]["clauseTypeId"].Value<string>()).FirstOrDefault();

                    var thoughtFieldType = thoughtType?["fieldTypes"].Where(fieldType => 
                        fieldType["id"].Value<string>() == document["AdditionalProperties"]["clauseTermId"].Value<string>()).FirstOrDefault();

                    document["AdditionalProperties"]["clauseTypeName"] = thoughtType?["name"].Value<string>();
                    document["AdditionalProperties"]["clauseTermName"] = thoughtFieldType?["name"].Value<string>();
                }
            }

            return item;
        }

        /// <summary>
        /// Gets the ontologies information from ThoughtTrace
        /// </summary>
        private async Task<JArray> GetThoughtTypesFromOntologies()
        {

            if(!_httpContextAccessor.HttpContext.Items.TryGetValue("Ontologies", out var ontologiesResponse))
            {
                ConnectorModel connectorModel = await _cosmosService.GetConnector("60", true);
                ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

                ontologiesResponse = await _dataMapping.GetAndMapResults<JToken>(connectorDocument, string.Empty, null, null, null);

                _httpContextAccessor.HttpContext.Items.Add("Ontologies", ontologiesResponse);
            }
            

            JArray thoughtTypes = new JArray();

            // As we don't need ontology metadata, I put together all tougtTypes to make search easier
            foreach (JToken ontology in (JToken)ontologiesResponse)
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

        public Dictionary<string, string> TTMap(Dictionary<string, string> cdmQueryParameters)
        {
            string clauseType = "";
            JArray clauseIds = new JArray();
            string keyword = "";

            JToken requestFilters = JToken.Parse(cdmQueryParameters["Filters"]);
            foreach (var filter in requestFilters)
            {
                var key = (string)filter.First.First;
                var value = filter.First.Next;

                if (key == "clauseTypeID")
                    clauseType = (string)value;

                if (key == "clauseTermIDs")
                    clauseIds = (JArray)value.First;

                if (key == "keyword")
                    keyword = (string)value;
            }

            //Remove default thoughtFilter if info is given
            if (String.IsNullOrEmpty(clauseType) || clauseIds?.Count < 1)
            {
                cdmQueryParameters.Remove("Filters");
                return cdmQueryParameters;
            }                
            else
            {
                cdmQueryParameters.Remove("thoughtFilters");
            }

            JObject thoughtFilters = new JObject();
            thoughtFilters.Add("operator", "and");

            JArray filters = new JArray();
            if (clauseIds.Count > 0)
                foreach (var id in clauseIds)
                {
                    JObject filterObject = new JObject();
                    JArray fieldTypes = new JArray();
                    JObject fieldTypesObject = new JObject();

                    fieldTypesObject.Add("thoughtTypeId", clauseType);
                    fieldTypesObject.Add("thoughtFieldTypeId", (string)id);
                    fieldTypes.Add(fieldTypesObject);
                    filterObject.Add("fieldTypes", fieldTypes);
                    filterObject.Add("operator", "exists");
                    filterObject.Add("stringValue", string.Empty);
                    filters.Add(filterObject);
                }
            else
            {
                JObject filterObject = new JObject();
                JArray fieldTypes = new JArray();
                JObject fieldTypesObject = new JObject();

                fieldTypesObject.Add("thoughtTypeId", clauseType);
                fieldTypes.Add(fieldTypesObject);
                filterObject.Add("fieldTypes", fieldTypes);
                filterObject.Add("operator", "exists");
                filterObject.Add("stringValue", string.Empty);
                filters.Add(filterObject);
            }
            thoughtFilters.Add("filters", filters);

            if (cdmQueryParameters.ContainsKey("threshold"))
            {
                JObject confidenceFilterObject = new JObject();
                confidenceFilterObject.Add("from", int.Parse(cdmQueryParameters["threshold"]));
                thoughtFilters.Add("confidenceFilter", confidenceFilterObject);
            }

            cdmQueryParameters.Remove("Filters");
            cdmQueryParameters.Add("thoughtFilters", thoughtFilters.ToString());
            cdmQueryParameters.Add("keyWord", keyword);
            return cdmQueryParameters;

        }
    }
}
