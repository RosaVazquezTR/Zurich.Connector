using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class DataMappingTransfer : AbstractDataMapping, IDataMapping
    {
        public DataMappingTransfer(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingTransfer> logger, ConnectorCosmosContext cosmosContext, IMapper mapper, IHttpBodyFactory factory, IHttpResponseFactory httpResponseFactory, IHttpContextAccessor contextAccessor, IOAuthApiRepository OAuthApirepository, OAuthOptions oAuthOptions, ILegalHomeAccessCheck legalHomeAccessCheck)
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

        public async override Task<T> GetAndMapResults<T>(ConnectorDocument connectorDocument, string transferToken, NameValueCollection query = null, Dictionary<string, string> headers = null)
        {
            T results = default(T);

            if (connectorDocument == null || transferToken == null)
            {
                return results;
            }

            ApiInformation apiInfo = new ApiInformation()
            {
                AppCode = connectorDocument.DataSource.appCode,
                HostName = connectorDocument.HostName,
                UrlPath = connectorDocument.Request.EndpointPath,
                AuthHeader = connectorDocument.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader,
                Token = null,
                Method = connectorDocument.Request.Method
            };

            CleanUpApiInformation(apiInfo);

            var transferTokenParam = new NameValueCollection() { { "transferToken", transferToken } };
            return await GetFromRepo<T>(apiInfo, connectorDocument, transferTokenParam);

        }
    }
}