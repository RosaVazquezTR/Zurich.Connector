using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            return appCode == KnownDataSources.thoughtTrace || appCode == KnownDataSources.thoughtTraceDMS;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            return item;
        }

        public async Task<Dictionary<string, string>> SetParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            //TODO: changing this from a specific connector id to a parameter in the data source
            if (!(connector.Response?.UseInternalSorting ?? false))
                return allParameters;


            allParameters = TTMap(allParameters);

            var thoughtFilters = JToken.Parse(allParameters["thoughtFilters"]);

            string clauseType = allParameters["clauseType"];
            allParameters.Remove("clauseType");

            String[] clauseTerms = Regex.Replace(allParameters["clauseTerms"], "[^0-9,]", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            allParameters.Remove("clauseTerms");

            //For the given clauseType/thoughTypeId, we first need to extract the provision thoughFieldTypeId from onotlogies
            var provisionID = "";
            provisionID = await GetProvisionThoughtFieldTypeIdFromOntologies(clauseType);
            if (provisionID == null)
                return null;
            allParameters["provisionID"] = provisionID;

            // provisionID filter allways have to be included

            JObject fieldType = new()
            {
                ["thoughtFieldTypeId"] = provisionID
            };

            JObject defaultProvisionFilter = new();
            defaultProvisionFilter["fieldTypes"] = new JArray(fieldType);
            //If theres no KW, provisionID filter goes with "exists", if there's KW, goes with "contains"
            if (allParameters.ContainsKey("keyWord") && String.IsNullOrEmpty(allParameters["keyWord"]))
            {
                defaultProvisionFilter["operator"] = "exists";
                defaultProvisionFilter["stringValue"] = String.Empty;
            }
            else
            {
                defaultProvisionFilter["operator"] = "contains";
                defaultProvisionFilter["stringValue"] = allParameters["keyWord"];
            }
            ((JArray)thoughtFilters["filters"]).Add(defaultProvisionFilter);

            //Then, foreach ClauseTerm
            clauseTerms = clauseTerms.Where(val => val != provisionID).ToArray();
            foreach (string clauseTerm in clauseTerms)
            {
                JObject fieldTypes = new()
                {
                    ["thoughtTypeId"] = clauseType,
                    ["thoughtFieldTypeId"] = clauseTerm.ToString()
                };
                JObject clauseTermFilters = new();
                clauseTermFilters["fieldTypes"] = new JArray(fieldTypes);
                clauseTermFilters["operator"] = "exists";
                clauseTermFilters["stringValue"] = String.Empty;
                ((JArray)thoughtFilters["filters"]).Add(clauseTermFilters);
            }
            allParameters["thoughtFilters"] = thoughtFilters.ToString();
            return allParameters;
        }

        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            //TODO: changing this from a specific connector id to a parameter in the data source
            if (connector.Id != "52" || connector.Id != "68")
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
            cdmQueryParameters.Remove("thoughtFilters");
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
            JObject thoughtFilters = new JObject();
            thoughtFilters.Add("operator", "and");

            JArray filters = new JArray();
            thoughtFilters.Add("filters", filters);

            if (cdmQueryParameters.ContainsKey("threshold"))
            {
                JObject confidenceFilterObject = new JObject();
                confidenceFilterObject.Add("from", int.Parse(cdmQueryParameters["threshold"]));
                thoughtFilters.Add("confidenceFilter", confidenceFilterObject);
                cdmQueryParameters.Remove("threshold");
            }

            cdmQueryParameters.Remove("Filters");
            cdmQueryParameters.Add("thoughtFilters", thoughtFilters.ToString());
            cdmQueryParameters.Add("keyWord", keyword);
            cdmQueryParameters.Add("clauseType", clauseType);
            cdmQueryParameters.Add("clauseTerms", clauseIds.ToString());
            return cdmQueryParameters;

        }
    }
}
