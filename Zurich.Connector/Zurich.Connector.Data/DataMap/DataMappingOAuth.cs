using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using AutoMapper;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingOAuth : DataMappingBase, IDataMapping
    {
        public DataMappingOAuth(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingOAuth> logger, ICosmosDocumentReader cosmosDocumentReader, IMapper mapper)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
            this._cosmosDocumentReader = cosmosDocumentReader;
            this._mapper = mapper;
        }

        public async override Task<T> Get<T>(ConnectorDocument dataTypeInformation, string transferToken = null, NameValueCollection query = null)
        {
            T results = default(T);

            if (dataTypeInformation == null)
            {
                return results;
            }

            AppToken token = await this.RetrieveToken(dataTypeInformation?.dataSource?.AppCode );

            if (!string.IsNullOrEmpty(token?.access_token))
            {
                ApiInformation apiInfo = new ApiInformation() { 
                    AppCode = dataTypeInformation.dataSource.AppCode, HostName = dataTypeInformation.hostName,
                    UrlPath = dataTypeInformation.request.endpointPath,
                    AuthHeader = dataTypeInformation.dataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader, Token = token };

                apiInfo.UrlPath = await this.UpdateUrl(apiInfo.UrlPath, dataTypeInformation);
                return await GetFromRepo<T>(apiInfo, dataTypeInformation, query);
            }

            return results;
        }
    }
}
