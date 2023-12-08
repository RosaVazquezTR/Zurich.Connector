using AutoMapper;
using Azure;
using Newtonsoft.Json.Linq;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Utils;

namespace Zurich.Connector.App.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDocumentDownloadService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appCode"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        Task<string> GetDocumentContent(string connectorId, string docId);
    }
    public class DocumentDownloadService : IDocumentDownloadService
    {
        private readonly IOAuthServices _OAuthService;
        private readonly IDataMapping _dataMapping;
        private readonly IDataMappingService _dataMappingService;
        private readonly OAuthOptions _oAuthOptions;
        private readonly IRepository _repository;
        private readonly IDataExtractionService _dataExtractionService;
        private readonly IMapper _mapper;

        public DocumentDownloadService(IOAuthServices OAuthService, IDataMappingFactory dataMappingFactory, IDataMappingService dataMappingService, OAuthOptions oAuthOptions, IRepository repository, IDataExtractionService dataExtractionService, IMapper mapper)
        {
            _OAuthService = OAuthService;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _dataMappingService = dataMappingService;
            _oAuthOptions = oAuthOptions;
            _repository = repository;
            _dataExtractionService = dataExtractionService;
            _mapper = mapper;
        }

        public async Task<string> GetDocumentContent(string connectorId, string docId)
        {
            string documentContent = "";
            List<DataSourceInformation> availableRegistrations;
            string dataBaseId = docId.Split("!")[0];
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "docId", docId },
                { "dbId", dataBaseId }
            };

            ConnectorModel connectorModel = await _dataMappingService.RetrieveProductInformationMap(connectorId, null, false);
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

            availableRegistrations = await _OAuthService.GetUserRegistrations();

            availableRegistrations = availableRegistrations?.FindAll(x => x.AppCode == connectorModel.DataSource.AppCode).ToList();

            if (availableRegistrations?.Count > 0)
            {
                var token = await _dataMapping.RetrieveToken(connectorModel.DataSource.AppCode);
                // This function is here just to obtain the customer_id for the UrlPath
                Dictionary<string, string> headerParameters = await _dataExtractionService.ExtractDataSource(null, parameters, null, connectorDocument);
                if (!string.IsNullOrEmpty(token?.AccessToken))
                {
                    ApiInformation apiInfo = new ApiInformation()
                    {
                        AppCode = connectorDocument.DataSource.appCode,
                        HostName = string.IsNullOrEmpty(connectorDocument.HostName) ? connectorDocument.DataSource.domain : connectorDocument.HostName,
                        UrlPath = connectorDocument.Request.EndpointPath,
                        AuthHeader = connectorDocument.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader,
                        Token = token,
                        Method = "get", // TODO: Once iManage (id 44) is migrated to use the new iManage GET endpoint we can replace the Method with connectorDocument.Request.Method
                        Headers = headerParameters
                    };

                    CleanUpApiInformation(apiInfo);

                    // TODO: If we want to add this feature for other connectors we should create a new connector definition for each data source that will
                    // support document download and have the urlpath defined there
                    apiInfo.UrlPath = apiInfo.UrlPath.Replace("/documents", $"/libraries/{dataBaseId}/documents/{docId}/download");

                    string documentStream = await _repository.DocumentDownloadMakeRequest(apiInfo);

                    documentContent = documentStream;
                }

            }
            else
            {
                throw new KeyNotFoundException("Invalid or unregistered connector");
            }

            return documentContent;
        }

        private void CleanUpApiInformation(ApiInformation apiInfo)
        {
            if (string.IsNullOrEmpty(apiInfo.HostName))
            {
                if (_oAuthOptions.Connections.ContainsKey(apiInfo.AppCode))
                {
                    var appCodeBaseUrl = _oAuthOptions.Connections[apiInfo.AppCode].BaseUrl;
                    apiInfo.HostName = UrlUtils.FormattingUrl(appCodeBaseUrl);
                }
            }
            else
            {
                apiInfo.HostName = UrlUtils.FormattingUrl(apiInfo.HostName);
            }
        }

    }
}
