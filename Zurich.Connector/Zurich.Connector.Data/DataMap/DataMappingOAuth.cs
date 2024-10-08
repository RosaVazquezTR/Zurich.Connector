﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.TenantData;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingOAuth : AbstractDataMapping, IDataMapping
    {
        public DataMappingOAuth(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingOAuth> logger, ConnectorCosmosContext cosmosContext, IMapper mapper, IHttpBodyFactory factory, IHttpResponseFactory httpResponseFactory, IHttpContextAccessor contextAccessor, IOAuthApiRepository OAuthApirepository, OAuthOptions oAuthOptions, ILegalHomeAccessCheck legalHomeAccessCheck, IConfiguration configuration, IAppConfigService appConfigService, IHttpClientFactory httpClientFactory, ISessionAccessor sessionAccessor)
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
            this._appConfigService = appConfigService;
            // Temporary measure to use the old way to obtain a token for HighQ, while highQ admin token is fixed in federated search
            // TODO: Remove this once the adminToken works in federated search and can be obtained from OAuth
            this._httpClientFactory = httpClientFactory;
            this._sessionAccessor = sessionAccessor;

        }

        public async override Task<T> GetAndMapResults<T>(ConnectorDocument connector, string transferToken, NameValueCollection query, Dictionary<string, string> headers, Dictionary<string, string> requestParameters, string domain = null)
        {
            // Given the current structure of WLIE API this is the easiest way to call its search endpoint
            // TODO: find a more generic way to do this, considering that WLIE doesn't require access token
            // NOTE: WLIE doesn't return error messages or any status different of 200,
            // in the future we will have to find a way to handle possible errors in the request, authentication or response
            if (connector.BasicAuthentication)
            {
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

                return await GetFromRepo<T>(apiInfo, connector, query, requestParameters);
            }

            var apiInformation = await this.CreateOAuthApiInformation(connector, domain);

            if (apiInformation != null)
            {
                apiInformation.Headers = headers;

                try
                {
                    return await GetFromRepo<T>(apiInformation, connector, query, requestParameters);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    throw;
                }
            }
            else
            {
                throw new AuthenticationException("Invalid or expired token");
            }
        }
    }
}
