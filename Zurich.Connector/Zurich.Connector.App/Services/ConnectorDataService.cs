using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.App.Utils;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using CommonServices = Zurich.Common.Services;
using Zurich.TenantData;
using Zurich.Connector.Data.Model;
using Microsoft.Extensions.Configuration;
using Zurich.Connector.App.Exceptions;
using Zurich.Common.Repositories;
using AppCodes = Zurich.Connector.Data.Constants.AppCodes;
using Newtonsoft.Json;

namespace Zurich.Connector.Data.Services
{
    /// <summary>
    /// Service to help facilitate the connectors data call. This should only be doing the work to get data and transform request and response objects.
    /// </summary>
    public interface IConnectorDataService
    {
        /// <summary>
        /// Get the data mapping results for a connector
        /// </summary>
        /// <param name="connectorIdentifier">The id or alias of the connector</param>
        /// <param name="hostname">The domain of the api being called</param>
        /// <param name="transferToken">The transfer token to pass with the api call, if needed</param>
        /// <param name="queryParameters">The query string parameters of request</param>
        /// <param name="domain">The domain of the api</param>
        /// <returns>Mapped data for the connector</returns>
        Task<dynamic> GetConnectorData(string connectorIdentifier, string hostname, string transferToken, Dictionary<string, string> queryParameters, bool retrievefilters);
    }

    public class ConnectorDataService : IConnectorDataService
    {
        private readonly IDataMappingFactory _dataMappingFactory;
        private readonly IDataMappingRepository _dataMappingRepo;
        private readonly ICosmosService _cosmosService;
        private readonly ILogger<ConnectorService> _logger;
        private readonly IMapper _mapper;
        private readonly IDataMappingService _dataMappingService;
        private readonly IConnectorDataSourceOperationsFactory _dataSourceOperationsFactory;
        private readonly IRegistrationService _registrationService;
        private readonly IDataExtractionService _dataExtractionService;
        private readonly ILegalHomeAccessCheck _legalHomeAccess;
        private readonly CommonServices.ITenantService _tenantService;
        private readonly IOAuthServices _OAuthService;
        private readonly IConfiguration _configuration;
        private readonly IOAuthApiRepository _OAuthApiRepository;
        private readonly ISessionAccessor _sessionAccessor;

        public ConnectorDataService(
            IDataMappingFactory dataMappingFactory,
            IDataMappingRepository dataMappingRepo,
            ILogger<ConnectorService> logger,
            IMapper mapper, ICosmosService cosmosService,
            IDataMappingService dataMappingService,
            IConnectorDataSourceOperationsFactory dataSourceOperationsFactory,
            IRegistrationService registrationService,
            IDataExtractionService dataExtractionService,
            ILegalHomeAccessCheck legalHomeAccess,
            CommonServices.ITenantService tenantService,
            IOAuthServices OAuthService,
            IConfiguration configuration,
            ISessionAccessor sessionAccessor)
        {
            _dataMappingFactory = dataMappingFactory;
            _dataMappingRepo = dataMappingRepo;
            _cosmosService = cosmosService;
            _logger = logger;
            _mapper = mapper;
            _dataMappingService = dataMappingService;
            _dataSourceOperationsFactory = dataSourceOperationsFactory;
            _registrationService = registrationService;
            _dataExtractionService = dataExtractionService;
            _legalHomeAccess = legalHomeAccess;
            _tenantService = tenantService;
            _OAuthService = OAuthService;
            _configuration = configuration;
            _sessionAccessor = sessionAccessor;
        }

        /// <summary>
        /// Get Connector Data
        /// </summary>
        /// <param name="connectionIdentifier">This can be connector id or alias</param>
        /// <param name="hostname"></param>
        /// <param name="transferToken"></param>
        /// <param name="queryParameters"></param>
        /// <param name="retrievefilters"></param>
        /// <returns>Connector data</returns>
        public async Task<dynamic> GetConnectorData(string connectionIdentifier, string hostname, string transferToken, Dictionary<string, string> queryParameters, bool retrieveFilters)
        {
            int instanceLimit = _configuration.GetValue<int>(AppSettings.InstanceLimit, 10);

            ConnectorModel connectorModel = await _dataMappingService.RetrieveProductInformationMap(connectionIdentifier, hostname, retrieveFilters);

            List<DataSourceInformation> availableRegistrations;

            if (_sessionAccessor.TenantId != Guid.Empty && _sessionAccessor.UserId == Guid.Empty)
            {
                availableRegistrations = await _OAuthService.GetAvailableRegistrations();
            }
            else
            {
                availableRegistrations = (await _OAuthService.GetUserRegistrations()).ToList();
            }

            availableRegistrations = availableRegistrations?.FindAll(x => x.AppCode == connectorModel.DataSource.AppCode).Take(instanceLimit).ToList<DataSourceInformation>();

            ValidateResultSize(queryParameters, connectorModel);
            ValidateQueryParams(queryParameters, connectorModel);

            // TODO: This is a legalhome workaround until legalhome uses OAuth
            if (string.IsNullOrEmpty(connectorModel.DataSource.Domain) && string.IsNullOrEmpty(hostname))
            {
                hostname = await GetBaseUrl(connectorModel);
                if (connectorModel != null && hostname != null)
                {
                    connectorModel.HostName = hostname;
                }
            }

            if (connectorModel == null)
            {
                return null;
            }
            if (connectorModel.AdvancedSearchSyntax != null && connectorModel.AdvancedSearchSyntax.Operators != null)
            {
                queryParameters = MapQueryAdvancedSearch(queryParameters, connectorModel);
            }
            else
            {
                if (queryParameters.ContainsKey("Query"))
                    queryParameters["Query"] = System.Web.HttpUtility.UrlDecode(queryParameters["Query"]);
            }

            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
            IDataMapping service = _dataMappingFactory.GetImplementation(connectorModel?.DataSource?.SecurityDefinition?.Type);

            // Needs to be dynamic because we can return an JToken or JArray
            dynamic data = null;
            int? resultSize = null;
            if (queryParameters.ContainsKey(ODataConstants.ResultSize) && !string.IsNullOrEmpty(queryParameters[ODataConstants.ResultSize]))
                resultSize = Convert.ToInt32(queryParameters[ODataConstants.ResultSize]);

            if (!resultSize.HasValue || resultSize.Value > 0)
            {
                if (connectorModel.DataSource.CombinedLocations)
                {
                    var instanceFilter = connectorDocument.Filters.Where(filter => filter.RequestParameter == "Instance.Filter").FirstOrDefault();
                    connectorDocument.Filters.Remove(instanceFilter);
                    foreach (DataSourceInformation currentRegistration in availableRegistrations)
                    {
                        FilterList availableInstance = new()
                        {
                            Id = currentRegistration.Domain,
                            Name = currentRegistration.Name
                        };
                        instanceFilter.FilterList.Add(availableInstance);
                    }
                    connectorDocument.Filters.Add(instanceFilter);

                    if (queryParameters.ContainsKey("Instance.Filter"))
                        availableRegistrations = availableRegistrations.FindAll(regs => queryParameters["Instance.Filter"].Contains(regs.Name));
                    if (availableRegistrations.Count > 0)
                        data = await GetDataForMultiInstanceConnector(connectorModel, connectorDocument, availableRegistrations, service, queryParameters, transferToken, hostname);
                    else
                        return data;

                }
                else
                {
                    //For connectors that do not support native offset we add the resultSize and the offset parameters
                    //Then in EnrichConnectorData we will trim the result
                    if (connectorModel.DataSource.ManualOffset && queryParameters.ContainsKey("resultSize") && queryParameters.ContainsKey("offset"))
                    {
                        queryParameters["resultSize"] = CalculateOffset(queryParameters).ToString();
                    }

                    // Validate if the connector is OneDrive to obtain user's url
                    if (connectorDocument.Id == "80")
                        queryParameters["Query"] = await GetAdditionalConnectorData(connectorModel, queryParameters["Query"]);

                    NameValueCollection mappedQueryParameters = MapQueryParametersFromDB(queryParameters, connectorModel);
                    Dictionary<string, string> headerParameters = await _dataExtractionService.ExtractDataSource(mappedQueryParameters, queryParameters, hostname, connectorDocument);

                    // As parameters are removed when making a request, we need a copy of them to addtitional requests if needed
                    NameValueCollection mappedQueryParametersCopy = new NameValueCollection(mappedQueryParameters);

                    // Thinking about using the instance domain instead of its name
                    queryParameters.Add("IHClauseBankCapabilityId", _configuration.GetValue<string>(AppSettings.IHClauseBankCapabilityId));
                    data = await service.GetAndMapResults<dynamic>(connectorDocument, transferToken, mappedQueryParameters, headerParameters, queryParameters);
                    data = await EnrichConnectorData(connectorModel, data, mappedQueryParametersCopy, queryParameters);
                }

                if (data == null)
                    return data;

                // TODO: Check if the count for TT response can be sent the same way the other connectors do so we can change this validation
                //if (data?.Count > 0)
                if (connectorModel.DataSource.CombinedLocations || (connectorModel.Response?.UseInternalSorting ?? false))
                    data.Documents = SortingResponseDocuments(data.Documents, connectorModel, queryParameters);
                if (data is JObject && data["AdditionalProperties"] != null && data.AdditionalProperties.pagination != null)
                {
                    var pagination_to = (int)data.AdditionalProperties.pagination.to;
                    var pagination_from = (int)data.AdditionalProperties.pagination.from;
                    data.Documents = PaginationResponseDocuments(data.Documents, pagination_from, pagination_to);
                }
            }

            // if there is no data because resultSize = 0 default to a JObject
            if (data == null)
            {
                data = new JObject();
            }

            if (retrieveFilters == true && data != null)
            {
                JToken mappingFilters = JToken.FromObject(connectorDocument.Filters);

                if (data is JArray)
                {
                    foreach (JObject instance in data)
                    {
                        instance[App.Constants.filters] = mappingFilters;
                    }
                }
                else if (data is JObject)
                    data[App.Constants.filters] = mappingFilters;
            }
            return data;
        }

        private void ValidateQueryParams(Dictionary<string, string> queryParameters, ConnectorModel connectorModel)
        {
            if (connectorModel.Response?.UseJsonTransformation ?? false) //Query Params validation only applies to CB & DMS connectors 52,68
            {
                try
                {
                    JObject logObjectTTRequest = new JObject();
                    logObjectTTRequest.Add("message", "TT Request started with following parameters:");

                    var requestJsonObject = JsonConvert.DeserializeObject<List<JObject>>(queryParameters["Filters"]);
                    foreach (JObject parameter in requestJsonObject)
                    {
                        logObjectTTRequest.Add(parameter["key"].ToString(), parameter["value"].ToString());
                    }

                    if (!logObjectTTRequest.ContainsKey("clauseTypeID"))
                    {
                        throw new RequiredParameterMissingException("Invalid request format. Missing filters object or ClauseTypeID parameter.");
                    }

                    if (queryParameters.ContainsKey("date.from"))
                    {
                        logObjectTTRequest.Add("date.from", queryParameters["date.from"]);
                    }

                    if (queryParameters.ContainsKey("threshold"))
                    {
                        logObjectTTRequest.Add("threshold", queryParameters["threshold"]);
                    }
                    else
                    {
                        logObjectTTRequest.Add("threshold", 0);
                    }
                    _logger.LogInformation(logObjectTTRequest.ToString());
                }
                catch
                {
                    throw new RequiredParameterMissingException("Invalid request format. Missing filters object or ClauseTypeID parameter.");
                }
            }
        }


        private void ValidateResultSize(Dictionary<string, string> queryParameters, ConnectorModel connectorModel)
        {
            if (queryParameters.ContainsKey(QueryParameters.ResultSize))
            {
                switch (connectorModel.DataSource.AppCode)
                {
                    case AppCodes.HighQ:
                        int maxRecordSizePerInstance = _configuration.GetValue(AppSettings.MaxRecordSizePerInstance, 1000);

                        if (Convert.ToInt64(queryParameters[QueryParameters.ResultSize]) > maxRecordSizePerInstance)
                            throw new MaxResultSizeException("Request exceeds maximum record size per instance of 1000");
                        break;
                    case AppCodes.LexisNexisUK:
                        //Lexis Nexis API only supports a maximum of 50 records per requests
                        if (Convert.ToInt64(queryParameters[QueryParameters.ResultSize]) > 50)
                            throw new MaxResultSizeException("Request exceeds maximum result size supported by Lexis Nexis of 50");
                        break;
                    default:
                        break;
                }
            }
        }

        private int CalculateOffset(Dictionary<string, string> queryParameters)
        {
            int resultSize = Convert.ToInt32(queryParameters["resultSize"]);
            int offset = Convert.ToInt32(queryParameters["offset"]);

            return resultSize + offset;
        }

        public Dictionary<string, string> MapQueryAdvancedSearch(Dictionary<string, string> cdmQueryParameters, ConnectorModel connectorModel)
        {
            if (!cdmQueryParameters.TryGetValue("Query", out string query))
            {
                var defaultParameter = connectorModel.Request?.Parameters?.FirstOrDefault(t => t.CdmName == "Query");
                if (defaultParameter == null || string.IsNullOrEmpty(defaultParameter.DefaultValue))
                    throw new RequiredParameterMissingException(string.Format("Missing required parameter: Query"));
                query = defaultParameter.DefaultValue;
            }

            string searchQuery = System.Web.HttpUtility.UrlDecode(query);
            cdmQueryParameters["Query"] = AdvancedSearchHandler.HandleOperator(searchQuery, connectorModel);
            return cdmQueryParameters;
        }

        public NameValueCollection MapQueryParametersFromDB(Dictionary<string, string> cdmQueryParameters, ConnectorModel connectorModel)
        {
            NameValueCollection modifiedQueryParameters = new NameValueCollection();
            var queryParameters = new Dictionary<string, string>();
            var sortParameters = new Dictionary<string, string>();

            if (cdmQueryParameters.Any())
            {
                if (connectorModel?.Pagination?.Available == true)
                {
                    cdmQueryParameters = SetupPagination(connectorModel, cdmQueryParameters);
                }

                if (connectorModel.Request?.Parameters != null)
                    queryParameters = (from param in cdmQueryParameters
                                       join requestParam in connectorModel.Request?.Parameters
                                       on param.Key.ToString().ToLower() equals requestParam.CdmName.ToLower()
                                       where requestParam.InClause != ODataConstants.OData && requestParam.InClause != InClauseConstants.Child
                                       select new { name = requestParam.Name, value = param.Value.ToString() }).ToDictionary(c => c.name, c => c.value);

                ValidateQueryParametersDataType(queryParameters, connectorModel.Request?.Parameters);

                if (connectorModel.Request?.Sorting != null)
                {
                    sortParameters = (from param in cdmQueryParameters
                                      join requestParam in connectorModel.Request?.Sorting?.Properties
                                      on param.Value.ToString().ToLower() equals requestParam.Name.ToLower()
                                      select new { name = requestParam.Element, value = requestParam.ElementValue.ToString() })
                                  .ToDictionary(c => c.name, c => c.value);
                }
            }

            if (ODataHandler.HasODataParams(connectorModel))
                ODataHandler.BuildQueryParams(cdmQueryParameters, connectorModel).ToList().ForEach(param => queryParameters.Add(param.Key, param.Value));

            // Throw if any required parameters are empty
            if (connectorModel.Request?.Parameters != null)
            {
                foreach (var parameter in connectorModel.Request?.Parameters)
                {
                    if (RequiredParametersCheck(parameter, queryParameters))
                        throw new RequiredParameterMissingException(string.Format("Missing required parameter: {0}", parameter.CdmName));
                }
            }

            // Add default parameters if not present in the request. ex: locale, ResultSize etc
            var defaultParameters = connectorModel.Request?.Parameters?.Where(t => DefaultParametersCheck(t, queryParameters))
                                .ToDictionary(c => c.Name, c => c.DefaultValue);

            // Add the default sort parameter specified in connector if necessary if not present in the request
            if (!sortParameters.Any() && connectorModel.Request?.Sorting != null)
            {
                sortParameters = (from requestParam in connectorModel.Request?.Sorting?.Properties
                                  where requestParam?.IsDefault ?? false
                                  select new { name = requestParam.Element, value = requestParam.ElementValue.ToString() })
                          .ToDictionary(c => c.name, c => c.value);
            }

            IEnumerable<KeyValuePair<string, string>> allParameters = new Dictionary<string, string>();

            if (queryParameters.Any())
                allParameters = allParameters.Concat(queryParameters);

            if (defaultParameters != null && defaultParameters.Any())
                allParameters = allParameters.Concat(defaultParameters);

            if (sortParameters.Any())
                allParameters = allParameters.Concat(sortParameters);

            allParameters = EnrichParametersSpecialCases(connectorModel, allParameters.ToDictionary(pair => pair.Key, pair => pair.Value)).Result;

            if (allParameters != null)
            {
                foreach (var parameter in allParameters)
                {
                    modifiedQueryParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return modifiedQueryParameters;
        }

        private void ValidateQueryParametersDataType(Dictionary<string, string> queryParameters, IEnumerable<ConnectorRequestParameterModel> connectorParameters)
        {
            foreach (var (key, value) in queryParameters)
            {
                var connectorParameter = connectorParameters.FirstOrDefault(query => query.Name == key);
                var isValid = false;

                switch (connectorParameter?.Type)
                {
                    case "string":
                        isValid = true;
                        break;
                    case "int":
                        isValid = int.TryParse(value, out _);
                        break;
                    case "bool":
                        isValid = bool.TryParse(value, out _);
                        break;
                    case "DateTime":
                        isValid = DateTime.TryParse(value, out _);
                        break;
                    case "object":
                        try
                        {
                            JContainer.Parse(value);
                            isValid = !int.TryParse(value, out _) && !bool.TryParse(value, out _);
                        }
                        catch (Exception)
                        {
                            isValid = false;
                        }
                        break;
                    default:
                        break;
                }

                if (!isValid)
                {
                    var invalidFilterMessage = connectorParameter?.CdmName == null ?
                        "Invalid data type provided for a filter." :
                        string.Format("Invalid data type provided for filter {0}.", connectorParameter.CdmName);
                    var invalidDataTypeMessage = connectorParameter?.Type == null ?
                        "Data type not found." :
                        string.Format("Value {0} cannot be parsed to {1}.", value, connectorParameter.Type);

                    throw new InvalidQueryParameterDataType($"{invalidFilterMessage} {invalidDataTypeMessage}");
                }
            }
        }

        private async Task<dynamic> GetBaseUrl(ConnectorModel connectorModel)
        {
            if (_legalHomeAccess.isLegalHomeUser())
            {
                if (connectorModel.DataSource.AppCode == KnownDataSources.iManage)
                {

                    var tenantApps = await _tenantService.GetTenantApplication(connectorModel.DataSource.AppCode);
                    return connectorModel.DataSource.Domain = new Uri(tenantApps.BaseUrl).Authority;
                }
            }
            return null;
        }

        private bool RequiredParametersCheck(ConnectorRequestParameterModel request, Dictionary<string, string> queryParameters)
        {
            return String.IsNullOrWhiteSpace(request.DefaultValue)
                && request.Required
                && !queryParameters.ContainsKey(request.Name);
        }

        private bool DefaultParametersCheck(ConnectorRequestParameterModel request, Dictionary<string, string> queryParameters)
        {
            return !String.IsNullOrWhiteSpace(request.DefaultValue)
                && !queryParameters.ContainsKey(request.Name)
                && request.InClause != ODataConstants.OData
                && request.InClause != InClauseConstants.Headers
                && request.InClause != InClauseConstants.Path;
        }

        private Dictionary<string, string> SetupPagination(ConnectorModel connectorModel, Dictionary<string, string> cdmQueryParameters)
        {
            // Ex: Office 365 uses 0 based offset numbering.
            if (connectorModel.Pagination?.IsZeroBasedOffset.HasValue == true &&
                connectorModel.Pagination?.IsZeroBasedOffset == false &&
                cdmQueryParameters.ContainsKey("Offset"))
            {
                cdmQueryParameters["Offset"] = (int.Parse(cdmQueryParameters["Offset"]) + 1).ToString();
            }
            return cdmQueryParameters;
        }

        /// <summary>
        /// Enriches the sort parameters using the app's corresponding data source operations service
        /// </summary>
        /// <param name="connector">The data connector</param>
        /// <param name="allParameters">All final parameters that will be sent in the request</param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> EnrichParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            var dataSourceOperationsService = _dataSourceOperationsFactory.GetDataSourceOperationsService(connector?.DataSource?.AppCode);
            if (dataSourceOperationsService != null)
                allParameters = await dataSourceOperationsService.SetParametersSpecialCases(connector, allParameters);
            else
                _logger.LogInformation("No data source operations service found for {appCode}", connector?.DataSource?.AppCode ?? "");
            return allParameters;
        }

        /// <summary>
        /// Enriches the connector data using the app's corresponding data source operations service
        /// </summary>
        /// <param name="connector">The data connector</param>
        /// <param name="data">The entity data</param>
        /// <returns></returns>
        private async Task<dynamic> EnrichConnectorData(ConnectorModel connector, dynamic data, NameValueCollection mappedQueryParameters = null, Dictionary<string, string> queryParameters = null, string instanceDomain = null)
        {
            var dataSourceOperationsService = _dataSourceOperationsFactory.GetDataSourceOperationsService(connector?.DataSource?.AppCode);

            // TODO: iManage endpoint no longer requires a manual offset implementation, but could be useful to keep this code
            if (connector.DataSource.ManualOffset && queryParameters.ContainsKey("resultSize") && queryParameters.ContainsKey("offset"))
            {
                data.Documents = ManualOffset(data, queryParameters);
            }

            if (connector?.FacetsInformation?.ConnectorId != null)
            {
                data = await AddFacetsFromAdditionalRequest(data, connector, mappedQueryParameters);
            }

            if (dataSourceOperationsService != null)
            {
                data = await dataSourceOperationsService.SetItemLink(connector.Info.EntityType, data, connector?.DataSource?.AppCode, connector.HostName);
                data = await dataSourceOperationsService.AddAditionalInformation(connector, data);
            }
            else
                _logger.LogInformation("No data source operations service found for {appCode}", connector?.DataSource?.AppCode ?? "");

            if (connector.DataSource.CombinedLocations)
            {
                foreach (var document in data.Documents)
                {
                    document.WebUrl = instanceDomain + document.WebUrl;
                }

            }
            return data;
        }

        private static dynamic ManualOffset(dynamic data, Dictionary<string, string> queryParameters)
        {
            JArray offsetDocuments = new JArray();
            var documents = data.Documents;
            int resultSize = Convert.ToInt32(queryParameters["resultSize"]);
            int offset = Convert.ToInt32(queryParameters["offset"]);

            if (documents.Count < resultSize)
            {
                resultSize = documents.Count;
            }

            for (int i = offset; i <= resultSize - 1; i++)
            {
                offsetDocuments.Add(documents[i]);
            }

            return offsetDocuments;
        }

        private async Task<dynamic> AddFacetsFromAdditionalRequest(dynamic data, ConnectorModel baseConnector, NameValueCollection mappedQueryParameters)
        {
            ConnectorModel connectorModel = await _cosmosService.GetConnector(baseConnector?.FacetsInformation?.ConnectorId, true);
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

            IDataMapping service = _dataMappingFactory.GetImplementation(connectorModel?.DataSource?.SecurityDefinition?.Type);
            JToken facetsResponse = await service.GetAndMapResults<JToken>(connectorDocument, null, mappedQueryParameters, null, null);

            ((JObject)data).TryAdd("facets", facetsResponse);
            return data;
        }

        private async Task<dynamic> GetDataForMultiInstanceConnector(ConnectorModel connectorModel, ConnectorDocument connectorDocument, List<DataSourceInformation> availableRegistrations, IDataMapping service, Dictionary<string, string> queryParameters, string transferToken, string hostname)
        {
            dynamic data = null;

            dynamic dataArray = new JObject();
            dataArray.Count = 0;
            dataArray.successUrls = new JArray();
            dataArray.failUrls = new JArray();
            dataArray.Documents = new JArray();

            // TODO: Turn this into a parallel process once HighQ Dogfood implementation for getting tokens is completed
            foreach (DataSourceInformation currentRegistration in availableRegistrations)
            {
                try
                {
                    NameValueCollection mappedQueryParameters = MapQueryParametersFromDB(queryParameters, connectorModel);
                    Dictionary<string, string> headerParameters = await _dataExtractionService.ExtractDataSource(mappedQueryParameters, queryParameters, hostname, connectorDocument);

                    data = await service.GetAndMapResults<dynamic>(connectorDocument, transferToken, mappedQueryParameters, headerParameters, queryParameters, currentRegistration.Domain);
                    data = await EnrichConnectorData(connectorModel, data, instanceDomain: currentRegistration.Domain);
                    dataArray.successUrls.Add(currentRegistration.Domain);
                }
                catch (Exception)
                {
                    dataArray.failUrls.Add(currentRegistration.Domain);
                }
                finally
                {
                    if (data.Documents != null)
                    {
                        foreach (var document in data.Documents)
                        {
                            dataArray.Documents.Add(document);
                        }
                    }
                    dataArray.Count += data.Count;
                }

            }

            return dataArray;
        }

        private static JArray PaginationResponseDocuments(JArray documents, int from, int to)
        {
            JArray aux = new JArray();
            for (int i = from - 1; i < to; i++)
            {
                documents[i]["AdditionalProperties"]["position"] = i + 1;
                aux.Add(documents[i]);
            }
            return aux;
        }

        private static JArray SortingResponseDocuments(JArray documents, ConnectorModel connectorModel, Dictionary<string, string> queryParameters)
        {
            // TODO: for the moment this code is too specific for TT and HighQ, for future connectors
            // that might use internal sorting we need to find a way to do it in a more generic way
            if (connectorModel.DataSource.CombinedLocations)
                documents = new JArray(documents.OrderByDescending(obj => (DateTime)obj["CreationDate"]));
            else if (connectorModel.Response?.UseInternalSorting ?? false)
            {
                string flatResStr = documents.ToString();
                flatResStr = flatResStr.Replace("score\": \"\"", "score\": 0.0");
                flatResStr = flatResStr.Replace("confidence\": \"\"", "confidence\": 0.0");
                documents = JArray.Parse(flatResStr);

                var keyWord = JToken.Parse(queryParameters["filters"])
                        ?.Where(filter => filter["key"].Value<string>() == "keyword").FirstOrDefault()?["value"];

                if (!String.IsNullOrEmpty(keyWord?.Value<string>()))
                    documents = new(documents.OrderByDescending(obj => (float)obj["AdditionalProperties"]["score"]).ThenBy(obj => obj["AdditionalProperties"]["fieldId"]));
                else
                    documents = new JArray(documents.OrderByDescending(obj => (float)obj["AdditionalProperties"]["confidence"]).ThenByDescending(obj => obj["AdditionalProperties"]["lastModifiedOn"]));
            }

            return documents;
        }

        /// <summary>
        /// At this point, it will only add the user's OneDrive path to the query
        /// </summary>
        /// <param name="connector">The data connector</param>
        /// <param name="query">The search query</param>
        /// <returns>The search query with the user's OneDrive path</returns>
        private async Task<string> GetAdditionalConnectorData(ConnectorModel connector, string query)
        {
            var dataSourceOperationsService = _dataSourceOperationsFactory.GetDataSourceOperationsService(connector?.DataSource?.AppCode);
            string urlPath = "";
            if (dataSourceOperationsService != null)
                urlPath = await dataSourceOperationsService.SetItemLink(connector.Info.EntityType, query, connector?.DataSource?.AppCode, connector.HostName);
            else
                _logger.LogInformation("No data source operations service found for {appCode}", connector?.DataSource?.AppCode ?? "");
            if (!string.IsNullOrWhiteSpace(urlPath))
            {
                query += $" (path:{urlPath})";
            }
            return query;
        }
    }
}
