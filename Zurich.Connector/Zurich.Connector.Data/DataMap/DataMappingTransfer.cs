using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using AutoMapper;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingTransfer : DataMappingBase, IDataMapping
    {
        public DataMappingTransfer(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingTransfer> logger, ICosmosDocumentReader cosmosDocumentReader, IMapper mapper)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
            this._cosmosDocumentReader = cosmosDocumentReader;
            this._mapper = mapper;
        }

        public async override Task<T> Get<T>(ConnectorModelEntity dataTypeInformation, string transferToken, NameValueCollection query = null)
        {
            T results = default(T);

            if (dataTypeInformation == null || transferToken == null)
            {
                return results;
            }

            ApiInformation apiInfo = new ApiInformation() { AppCode = dataTypeInformation.DataSource.AppCode, 
                                                            HostName = dataTypeInformation.HostName, 
                                                            UrlPath = dataTypeInformation.Request.EndpointPath, 
                                                            AuthHeader = dataTypeInformation.DataSource.SecurityDefinition.DefaultSecurityDefinition.AuthorizationHeader, 
                                                            Token = null };
            apiInfo.UrlPath = await this.UpdateUrl(apiInfo.UrlPath, dataTypeInformation, transferToken);

            var transferTokenParam = new NameValueCollection() { { "transferToken", transferToken } }; 
            return await GetFromRepo<T>(apiInfo, dataTypeInformation, transferTokenParam);
           
        }
    }
}