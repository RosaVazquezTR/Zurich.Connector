﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using AutoMapper;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using System.Security.Authentication;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingOAuth : DataMappingBase, IDataMapping
    {
        public DataMappingOAuth(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingOAuth> logger, ConnectorCosmosContext cosmosContext, IMapper mapper)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
            this._cosmosContext = cosmosContext;
            this._mapper = mapper;
        }

        public async override Task<T> Get<T>(ConnectorDocument connector, string transferToken = null, NameValueCollection query = null)
        {
            var token = await this.RetrieveToken(connector?.dataSource?.appCode,
                                                  connector?.dataSource?.appType,
                                                  connector?.dataSource?.locale,
                                                  connector?.dataSource?.securityDefinition?.defaultSecurityDefinition?.grantType,
                                                  connector?.dataSource?.securityDefinition?.defaultSecurityDefinition?.sendCredentialsInBody);         
            
            if (!string.IsNullOrEmpty(token?.access_token))
            {
                ApiInformation apiInfo = new ApiInformation()
                {
                    AppCode = connector.dataSource.appCode,
                    HostName = string.IsNullOrEmpty(connector.hostName) ? connector.dataSource.domain : connector.hostName,
                    UrlPath = connector.request.endpointPath,
                    AuthHeader = connector.dataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader,
                    Token = token
                };

                apiInfo.UrlPath = await this.UpdateUrl(apiInfo.UrlPath, connector);
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
