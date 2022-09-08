using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.ProductData.Models;
using OAuthAPITokenResponse = Zurich.Common.Models.OAuth.OAuthAPITokenResponse;
using System.Text;

namespace Zurich.Connector.Data.DataMap
{
public class DataMappingBasic : AbstractDataMapping, IDataMapping
{
    public DataMappingBasic(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, OAuthOptions oAuthOptions, ConnectorCosmosContext cosmosContext, IHttpBodyFactory httpBodyFactory, IHttpResponseFactory httpResponseFactory, ILogger<DataMapping> logger, IHttpContextAccessor contextAccessor, IOAuthApiRepository OAuthApirepository, ILegalHomeAccessCheck legalHomeAccessCheck, IConfiguration configuration)
    {
        this._repository = repository;
        this._dataMappingRepository = dataMappingRepository;
        this._oAuthService = oAuthService;
        this._oAuthOptions = oAuthOptions;
        this._logger = logger;
        this._httpResponseFactory = httpResponseFactory;
        this._httpBodyFactory = httpBodyFactory;
        this._contextAccessor = contextAccessor;
        this._oAuthApirepository = OAuthApirepository;
        this._legalHomeAccessCheck = legalHomeAccessCheck;
        this._configuration = configuration;
        this._cosmosContext = cosmosContext;
    }
    public async override Task<T> GetAndMapResults<T>(ConnectorDocument connector, string transferToken, NameValueCollection query, Dictionary<string, string> headers, Dictionary<string, string> requestParameters, string domain = null)
    {    
            var appinfoDetails = _oAuthOptions.Connections.SingleOrDefault(x => x.Key.Equals(connector.DataSource.appCode, StringComparison.OrdinalIgnoreCase));
            string svcCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(appinfoDetails.Value.Id + ":" + appinfoDetails.Value.Secret));
            connector.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader = "Basic " + svcCredentials;
            headers.Add("Authorization", connector.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader);

            ApiInformation apiInfo = new ApiInformation()
            {
                AppCode = connector.DataSource.appCode,
                HostName = string.IsNullOrEmpty(connector.HostName) ? connector.DataSource.domain : connector.HostName,
                UrlPath = connector.Request.EndpointPath,
                AuthHeader = connector.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader,
                Method = connector.Request.Method,
                Headers = headers
            };

            CleanUpApiInformation(apiInfo);

            try
            {
                return await GetFromRepo<T>(apiInfo, connector, query, requestParameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

        return default(T);
    }

}
    
    

}
