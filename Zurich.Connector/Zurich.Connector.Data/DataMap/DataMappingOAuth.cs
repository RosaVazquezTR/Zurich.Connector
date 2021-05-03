using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.Data.DataMap
{
    public class DataMappingOAuth : DataMappingBase, IDataMapping
    {
        public DataMappingOAuth(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingOAuth> logger)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
        }

        public async override Task<T> Get<T>(DataMappingClass dataTypeInformation, string transferToken = null)
        {
            T results = default(T);

            if (dataTypeInformation == null)
            {
                return results;
            }

            AppToken token = await this.RetrieveToken(dataTypeInformation.AppCode);

            if (!string.IsNullOrEmpty(token?.access_token))
            {
                ApiInformation apiInfo = new ApiInformation() { AppCode = dataTypeInformation.AppCode, HostName = dataTypeInformation.Api.Hostname, UrlPath = dataTypeInformation.Api.Url, AuthHeader = dataTypeInformation.Api.AuthHeader, Token = token };

                apiInfo.UrlPath = await this.UpdateUrl(apiInfo.UrlPath, dataTypeInformation);
                return await GetFromRepo<T>(apiInfo, dataTypeInformation);
            }

            return results;
        }
    }
}
