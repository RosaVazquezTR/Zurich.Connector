using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        public DataMappingTransfer(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMapping> logger)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
        }

        public async override Task<T> Get<T>(string appCode, DataType dataType, DataMappingClass dataTypeInformation, string transferToken)
        {
            T results = default(T);

            //DataMappingClass dataTypeInformation = await this.RetrieveProductInformationMap(appCode, dataType); //move up a service

            if (dataTypeInformation == null || transferToken == null)
            {
                return results;
            }

            ApiInformation apiInfo = new ApiInformation() { AppCode = dataTypeInformation.AppCode, HostName = "us.practicallaw.qed.thomsonreuters.com", UrlPath = dataTypeInformation.Api.Url, AuthHeader = dataTypeInformation.Api.AuthHeader, Token = null };
            apiInfo.UrlPath = await this.UpdateUrl(appCode, apiInfo.UrlPath, dataTypeInformation, transferToken);

            var response = await _repository.Get(apiInfo, transferToken);

            if (!string.IsNullOrWhiteSpace(response))
            {
                results = await MapToCDM<T>(response, dataTypeInformation.ResultLocation, dataTypeInformation.Mapping);
            }
            
            return results;
        }
    }
}