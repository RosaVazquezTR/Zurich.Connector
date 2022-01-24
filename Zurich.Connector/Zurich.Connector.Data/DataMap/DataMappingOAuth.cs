using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingOAuth : AbstractDataMapping, IDataMapping
    {
        public DataMappingOAuth(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingOAuth> logger, ConnectorCosmosContext cosmosContext, IMapper mapper, IHttpBodyFactory factory, IHttpResponseFactory httpResponseFactory, IHttpContextAccessor contextAccessor, IOAuthApiRepository OAuthApirepository, OAuthOptions oAuthOptions, ILegalHomeAccessCheck legalHomeAccessCheck, IConfiguration configuration)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
            this._cosmosContext = cosmosContext;
            this._mapper = mapper;
            this._httpBodyFactory = factory;
            this._httpResponseFactory = httpResponseFactory;
            this._contextAccessor = contextAccessor;
            this._oAuthApirepository = OAuthApirepository;
            this._oAuthOptions = oAuthOptions;
            this._legalHomeAccessCheck = legalHomeAccessCheck;
            this._configuration = configuration;
        }

        public async override Task<T> GetAndMapResults<T>(ConnectorDocument connector, string transferToken, NameValueCollection query, Dictionary<string, string> headers, Dictionary<string, string> requestParameters)
        {
            var token = await this.RetrieveToken(connector?.DataSource?.appCode,
                                                  connector?.DataSource?.appType,
                                                  connector?.DataSource?.locale,
                                                  connector?.DataSource?.securityDefinition?.defaultSecurityDefinition?.grantType,
                                                  connector?.DataSource?.securityDefinition?.defaultSecurityDefinition?.sendCredentialsInBody,
                                                  connector?.DataSource?.productType);         
            
            if (!string.IsNullOrEmpty(token?.AccessToken))
            {
                ApiInformation apiInfo = new ApiInformation()
                {
                    AppCode = connector.DataSource.appCode,
                    HostName = string.IsNullOrEmpty(connector.HostName) ? connector.DataSource.domain : connector.HostName,
                    UrlPath = connector.Request.EndpointPath,
                    AuthHeader = connector.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader,
                    Token = token,
                    Method = connector.Request.Method,
                    Headers = headers
            };

                CleanUpApiInformation(apiInfo);

                try
                {
                    return await GetFromRepo<T>(apiInfo, connector, query, requestParameters);
                } catch(Exception e)
                {
                    _logger.LogError(e.Message);
                    throw;
                }
            }
            else
            {
                throw new AuthenticationException("Invalid token");
            }
        }
    }
}
