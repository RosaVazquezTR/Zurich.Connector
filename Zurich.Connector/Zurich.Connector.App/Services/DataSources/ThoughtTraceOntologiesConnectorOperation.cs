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
using Zurich.Connector.App.Exceptions;
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
            if (!(connector.Response?.UseJsonTransformation ?? false))
                return allParameters;


            allParameters = TTRequestMap(allParameters);

            var thoughtFilters = JToken.Parse(allParameters["thoughtFilters"]);

            string clauseType = allParameters["clauseType"];
            allParameters.Remove("clauseType");

            String[] clauseTerms = Regex.Replace(allParameters["clauseTerms"], "[^0-9,]", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            allParameters.Remove("clauseTerms");

            List<string> keyWords = allParameters["keyWord"].Split(",_").ToList();

            //For the given clauseType/thoughTypeId, we first need to extract the provision thoughFieldTypeId from onotlogies
            var provisionID = "";
            provisionID = await GetProvisionThoughtFieldTypeIdFromOntologies(clauseType, connector.OntologiesInformation.ConnectorId);
            if (provisionID == null)
                return null;
            allParameters["provisionID"] = provisionID;

            // A provisionID filter allways have to be included. 
            // With no keyword, 1 "exists" filter
            // With n keywords, n "contains" filters

            foreach (string kw in keyWords)
            {
                JObject fieldType = new()
                {
                    ["thoughtFieldTypeId"] = provisionID
                };

                JObject defaultProvisionFilter = new();
                defaultProvisionFilter["fieldTypes"] = new JArray(fieldType);
                //If theres no KW, provisionID filter goes with "exists", if there's KW, goes with "contains"
                if (String.IsNullOrEmpty(kw))
                {
                    defaultProvisionFilter["operator"] = "exists";
                    defaultProvisionFilter["stringValue"] = String.Empty;
                }
                else
                {
                    defaultProvisionFilter["operator"] = "contains";
                    defaultProvisionFilter["stringValue"] = kw;
                }
                ((JArray)thoughtFilters["filters"]).Add(defaultProvisionFilter);
            }

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
            if (!(connector.Response?.UseJsonTransformation ?? false))
                return item;

            JArray thoughtTypes = await GetThoughtTypesFromOntologies(connector.OntologiesInformation.ConnectorId);

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
        private async Task<JArray> GetThoughtTypesFromOntologies(string OntologiesConnectorId)
        {

            if (!_httpContextAccessor.HttpContext.Items.TryGetValue("Ontologies", out var ontologiesResponse))
            {
                ConnectorModel connectorModel = await _cosmosService.GetConnector(OntologiesConnectorId, true);
                ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

                ontologiesResponse = await _dataMapping.GetAndMapResults<JToken>(connectorDocument, string.Empty, null, null, null);

                _httpContextAccessor.HttpContext.Items.Add("Ontologies", ontologiesResponse);
            }


            JArray thoughtTypes = new JArray();

            // As we don't need ontology metadata, I put together all tougtTypes to make search easier
            foreach (JToken ontology in (JToken)ontologiesResponse)
            {
                foreach (JToken thoughtType in ontology["thoughtTypes"])
                    thoughtTypes.Add(thoughtType);
            }

            return thoughtTypes;
        }

        /// <summary>
        /// Gets the "Provision" thought id field given a thoughtTypeId information from ontologies
        /// </summary>
        public async Task<string> GetProvisionThoughtFieldTypeIdFromOntologies(string thoughtTypeId, string OntologiesConnectorID)
        {
            JArray thoughtTypes = await GetThoughtTypesFromOntologies(OntologiesConnectorID);

            // In case "Provision" field doesn't exists (feature included with MAN ontologies), we consider the field with the name of the clauseType
            // as the provision (technically, it should contains only that fieldType). 
            var thoughtType = thoughtTypes.FirstOrDefault(thoughtType => thoughtType["id"].Value<string>() == thoughtTypeId);
            var provisionThought = thoughtType?["fieldTypes"].Where(fieldType => fieldType["name"].Value<string>() == "Provision").FirstOrDefault() 
                ?? thoughtType?["fieldTypes"].Where(fieldType => fieldType["name"].Value<string>() == thoughtType?["name"].Value<string>()).FirstOrDefault();
            
            return provisionThought?["id"].Value<string>();
        }

        public Dictionary<string, string> TTRequestMap(Dictionary<string, string> cdmQueryParameters)
        {
            cdmQueryParameters.Remove("thoughtFilters");
            string clauseType = "";
            JArray clauseIds = new JArray();
            string originalKeyword = "";
            List<string> keyword = new List<string>();

            JToken requestFilters = JToken.Parse(cdmQueryParameters["Filters"]);
            foreach (var filter in requestFilters)
            {
                var key = filter["key"].Value<string>();

                if (key == "clauseTypeID")
                    clauseType = filter["value"].Value<string>();

                else if (key == "clauseTermIDs")
                    clauseIds = (JArray)filter["value"];

                else if (key == "keyword")
                {
                    var value = filter["value"].Value<string>();
                    originalKeyword = Regex.Replace(value, @"\s+", " ").Trim(); //Remove extra spaces between words.
                    if (validateQuery(originalKeyword))
                    {
                        var enclosedTexts = Regex.Matches(originalKeyword, @"\" + (char)34 + @"(.+?)\" + (char)34)
                       .Cast<Match>()
                       .Select(m => m.Groups[1].Value.Trim())
                       .ToList();
                        foreach (string text in enclosedTexts)
                        {
                            keyword.Add(text);
                            originalKeyword = originalKeyword.Replace(text, String.Empty);
                        }
                        char[] bannedChars = { ',', '.', '"', ':', ';'};
                        string regexPattern = "[" + Regex.Escape(new string(bannedChars)) + "]";
                        originalKeyword = Regex.Replace(originalKeyword, regexPattern, "").Trim();
                        
                        if (!String.IsNullOrEmpty(originalKeyword))
                            keyword.AddRange(originalKeyword.Split(' ').ToList());
                    }
                    else
                        throw new InvalidQueryFormatException("Query contains invalid format.");
                }
            }
            JObject thoughtFilters = new JObject();
            if (keyword.Count > 1)
                thoughtFilters.Add("operator", "or");
            else
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
            cdmQueryParameters.Add("keyWord", string.Join(",_", keyword));
            cdmQueryParameters.Add("clauseType", clauseType);
            cdmQueryParameters.Add("clauseTerms", clauseIds.ToString());
            return cdmQueryParameters;

        }

        public static bool validateQuery(string query)
        {
            // The "" operator strictly open and close. 
            int countQuotes = query.Count(f => (f == '"'));
            var enclosedTexts = Regex.Matches(query, @"\" + (char)34 + @"(.+?)\" + (char)34);

            if ((countQuotes % 2 != 0) || (countQuotes/2 != enclosedTexts.Count))
                return false;
            return true;
        }

    }
}
