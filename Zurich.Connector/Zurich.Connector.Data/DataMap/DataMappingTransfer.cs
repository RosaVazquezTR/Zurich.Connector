using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Data.Utils;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingTransfer : AbstractDataMapping, IDataMapping
    {
        public DataMappingTransfer(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingTransfer> logger, ConnectorCosmosContext cosmosContext, IMapper mapper, IHttpBodyFactory factory, IHttpResponseFactory httpResponseFactory, IHttpContextAccessor contextAccessor, IOAuthApiRepository OAuthApirepository, OAuthOptions oAuthOptions, ILegalHomeAccessCheck legalHomeAccessCheck, IConfiguration configuration)
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

        public async override Task<T> GetAndMapResults<T>(ConnectorDocument connectorDocument, string transferToken, NameValueCollection query, Dictionary<string, string> headers, Dictionary<string, string> requestParameters, string domain = null)
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
                Method = connectorDocument.Request.Method,
                Headers = headers
            };

            CleanUpApiInformation(apiInfo);

            var transferTokenParam = new NameValueCollection() { { "transferToken", transferToken } };
            return await GetFromRepo<T>(apiInfo, connectorDocument, transferTokenParam, requestParameters);

        }
        /// <summary>
		/// Will make sure ApiInformation is setup correctly
		/// </summary>
		/// <param name="info">The apiInformation to check</param>
		public override void CleanUpApiInformation(ApiInformation info)
        {
            // can probably get rid of this when OAuth is setup
            if (string.IsNullOrEmpty(info.HostName))
            {
                if (_oAuthOptions.Connections.ContainsKey(info.AppCode))
                {
                    var appCodeBaseUrl = _oAuthOptions.Connections[info.AppCode].TransferUrl;
                    //because we use a url builder we need to drop the https, however we need this for the token information
                    info.HostName = UrlUtils.FormattingUrl(appCodeBaseUrl);
                }
            }
            else
            {
                info.HostName = UrlUtils.FormattingUrl(info.HostName);
            }
        }
    }
}