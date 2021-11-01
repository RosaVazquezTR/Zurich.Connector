using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        public DataMappingOAuth(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingOAuth> logger, ConnectorCosmosContext cosmosContext, IMapper mapper, IHttpBodyFactory factory, IHttpResponseFactory httpResponseFactory, IHttpContextAccessor contextAccessor, IOAuthApiRepository OAuthApirepository, OAuthOptions oAuthOptions, ILegalHomeAccessCheck legalHomeAccessCheck)
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

        }

        public async override Task<T> GetAndMapResults<T>(ConnectorDocument connector, string transferToken = null, NameValueCollection query = null, Dictionary<string, string> headers = null)
        {
            var token = await this.RetrieveToken(connector?.DataSource?.appCode,
                                                  connector?.DataSource?.appType,
                                                  connector?.DataSource?.locale,
                                                  connector?.DataSource?.securityDefinition?.defaultSecurityDefinition?.grantType,
                                                  connector?.DataSource?.securityDefinition?.defaultSecurityDefinition?.sendCredentialsInBody,
                                                  connector?.DataSource?.productType);         
            
            if (!string.IsNullOrEmpty(token?.access_token))
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
                    return await GetFromRepo<T>(apiInfo, connector, query);
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
