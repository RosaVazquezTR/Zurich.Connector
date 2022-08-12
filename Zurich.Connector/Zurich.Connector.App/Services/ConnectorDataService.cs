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
            IConfiguration configuration)
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
            int MaxRecordSizePerInstance = _configuration.GetValue<int>(AppSettings.MaxRecordSizePerInstance, 1000);

            ConnectorModel connectorModel = await _dataMappingService.RetrieveProductInformationMap(connectionIdentifier, hostname, retrieveFilters);
            List<DataSourceInformation> availableRegistrations = await _OAuthService.GetUserRegistrations();
            availableRegistrations = availableRegistrations?.FindAll(x => x.AppCode == connectorModel.DataSource.AppCode).Take(instanceLimit).ToList<DataSourceInformation>();
             
            queryParameters = _dataMappingService.UpdateOffset(connectorModel.DataSource.AppCode, availableRegistrations, queryParameters);

            if (queryParameters.ContainsKey(QueryParameters.ResultSize))
            {
                if (Convert.ToInt64(queryParameters[QueryParameters.ResultSize]) > MaxRecordSizePerInstance)
                    throw new MaxResultSizeException("Request exceeds maximum record size per instance of 1000");
            }

            // TODO: This is a legalhome workaround until legalhome uses OAuth
            if (string.IsNullOrEmpty(connectorModel.DataSource.Domain) && string.IsNullOrEmpty(hostname))
            {
                hostname = await GetBaseUrl(connectorModel);
                if (connectorModel != null)
                {
                    connectorModel.HostName = hostname;
                }
            }

            if (connectorModel == null)
            {
                return null;
            }
            //Para pruebas solamente
            if (connectorModel.Id == "51") { 
                connectorModel.AdvancedSyntax = new ConnectorAdvancedSyntaxModel();
                connectorModel.AdvancedSyntax.Operators = new AdvancedSyntaxOperatorModel();
                connectorModel.AdvancedSyntax.Operators.And = "AND";
                connectorModel.AdvancedSyntax.Operators.Or = "OR";
                connectorModel.AdvancedSyntax.Operators.Wildcard = "*";
                connectorModel.AdvancedSyntax.Operators.TextualOcurrence = "\"";
                connectorModel.AdvancedSyntax.Operators.Not = "NOT ";
                connectorModel.AdvancedSyntax.Operators.TermGrouping = "(";
                connectorModel.AdvancedSyntax.Operators.Proximity = "NEAR(n)";
            }
            if (connectorModel.AdvancedSyntax!= null && connectorModel.AdvancedSyntax.Operators != null) 
            {
                queryParameters = MapQueryAdvancedSearch(queryParameters, connectorModel);
            }

           NameValueCollection mappedQueryParameters = MapQueryParametersFromDB(queryParameters, connectorModel);
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
            Dictionary<string, string> headerParameters = await _dataExtractionService.ExtractDataSource(mappedQueryParameters, queryParameters, hostname, connectorDocument);
            IDataMapping service = _dataMappingFactory.GetImplementation(connectorModel?.DataSource?.SecurityDefinition?.Type);

            // Needs to be dynamic because we can return an JToken or JArray
            dynamic data = null;
            int? resultSize = null;
            if (queryParameters.ContainsKey(ODataConstants.ResultSize) && !string.IsNullOrEmpty(queryParameters[ODataConstants.ResultSize]))
                resultSize = Convert.ToInt32(queryParameters[ODataConstants.ResultSize]);

            if (!resultSize.HasValue || resultSize.Value > 0)
            {
                data = await service.GetAndMapResults<dynamic>(connectorDocument, transferToken, mappedQueryParameters, headerParameters, queryParameters);
                data = await EnrichConnectorData(connectorModel, data);
                
                if (data == null)
                    return data;
            }

            // if there is no data because resultSize = 0 default to a JObject
            if (data == null)
            {
                data = new JObject();
            }

            if (retrieveFilters == true && data != null)
            {

                JToken mappingFilters = JToken.FromObject(connectorDocument.Filters);
                data[Constants.filters] = mappingFilters;
            }
            return data;
        }

        public Dictionary<string, string> MapQueryAdvancedSearch(Dictionary<string, string> cdmQueryParameters, ConnectorModel connectorModel)
        {
            string searchQuery = System.Web.HttpUtility.UrlDecode(cdmQueryParameters["Query"]);
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

            allParameters = EnrichSortParametersSpecialCases(connectorModel, allParameters.ToDictionary(pair => pair.Key, pair => pair.Value)).Result;

            if (allParameters != null)
            {
                foreach (var parameter in allParameters)
                {
                    modifiedQueryParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return modifiedQueryParameters;
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

        private bool DefaultParametersCheck(ConnectorRequestParameterModel request, Dictionary<string, string> queryParameters)
        {
            return !String.IsNullOrWhiteSpace(request.DefaultValue)
                && !queryParameters.ContainsKey(request.Name)
                && request.InClause != ODataConstants.OData
                && request.InClause != InClauseConstants.Headers;
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
        private async Task<Dictionary<string, string>> EnrichSortParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            var dataSourceOperationsService = _dataSourceOperationsFactory.GetDataSourceOperationsService(connector?.DataSource?.AppCode);
            if (dataSourceOperationsService != null)
                allParameters = await dataSourceOperationsService.SetSortParameters(allParameters);
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
        private async Task<dynamic> EnrichConnectorData(ConnectorModel connector, dynamic data)
        {
            var dataSourceOperationsService = _dataSourceOperationsFactory.GetDataSourceOperationsService(connector?.DataSource?.AppCode);
            if (dataSourceOperationsService != null)
                data = await dataSourceOperationsService.SetItemLink(connector.Info.EntityType, data, connector?.DataSource?.AppCode, connector.HostName);
            else
                _logger.LogInformation("No data source operations service found for {appCode}", connector?.DataSource?.AppCode ?? "");
            return data;
        }
    }
}
