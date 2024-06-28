using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.TenantData;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingOAuthWithTransfer : AbstractDataMapping, IDataMapping
    {
        public DataMappingOAuthWithTransfer(
            IRepository repository,
            IDataMappingRepository dataMappingRepository,
            IOAuthService oAuthService,
            ILogger<DataMappingOAuth> logger,
            ConnectorCosmosContext cosmosContext,
            IMapper mapper, IHttpBodyFactory factory,
            IHttpResponseFactory httpResponseFactory,
            IHttpContextAccessor contextAccessor,
            IOAuthApiRepository OAuthApirepository,
            OAuthOptions oAuthOptions,
            ILegalHomeAccessCheck legalHomeAccessCheck,
            IConfiguration configuration,
            IAppConfigService appConfigService,
            IHttpClientFactory httpClientFactory,
            ISessionAccessor sessionAccessor)
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
            this._sessionAccessor = sessionAccessor;
        }

        public async override Task<T> GetAndMapResults<T>(ConnectorDocument connector, string transferToken, NameValueCollection query, Dictionary<string, string> headers, Dictionary<string, string> requestParameters, string domain = null)
        {
            var identityTransferToken = await _oAuthApirepository.GetTransferToken();

            if (identityTransferToken != null)
            {
                identityTransferToken = identityTransferToken.Replace("\"", "");
                var apiInformation = await this.CreateOAuthApiInformation(connector, domain);

                if (apiInformation != null)
                {
                    headers.Add("X-AUTH-CODE", identityTransferToken);
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
            else
            {
                throw new AuthenticationException("Invalid transfer token");
            }
        }
    }
}
