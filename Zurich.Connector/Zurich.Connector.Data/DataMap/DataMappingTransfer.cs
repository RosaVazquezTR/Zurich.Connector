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

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingTransfer : DataMappingBase, IDataMapping
    {
        public DataMappingTransfer(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingTransfer> logger)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
        }

        public async override Task<T> Get<T>(DataMappingClass dataTypeInformation, string transferToken, NameValueCollection query = null)
        {
            T results = default(T);

            if (dataTypeInformation == null || transferToken == null)
            {
                return results;
            }

            ApiInformation apiInfo = new ApiInformation() { AppCode = dataTypeInformation.AppCode, HostName = dataTypeInformation.Api.Hostname, UrlPath = dataTypeInformation.Api.Url, AuthHeader = dataTypeInformation.Api.AuthHeader, Token = null };
            apiInfo.UrlPath = await this.UpdateUrl(apiInfo.UrlPath, dataTypeInformation, transferToken);

            var transferTokenParam = new NameValueCollection() { { "transferToken", transferToken } }; 
            return await GetFromRepo<T>(apiInfo, dataTypeInformation, transferTokenParam);
           
        }
    }
}