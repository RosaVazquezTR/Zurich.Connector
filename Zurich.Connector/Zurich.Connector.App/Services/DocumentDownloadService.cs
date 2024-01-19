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
            string dataBaseId = "";
            List<DataSourceInformation> availableRegistrations;
            DataSourceInformation selectedRegistration;

            if (connectorId == "44")
                dataBaseId = docId.Split("!")[0];
            else // connectors 14, 80 & 47
            {
                dataBaseId = docId.Split(",")[0];
                docId = docId.Split(",")[1];
            }
            // This dictionary is not really being used, but _dataExtractionService.ExtractDataSource needs queryParameters not null to work
            // We don't need queryParameters for any document content call, so I considered better to send this parameters instead of modifying ExtractDataSource
            Dictionary<string, string> parameters = new()
            {
                { "dataBaseId", dataBaseId },
                { "docId", docId }
            };

            ConnectorModel connectorModel = await _dataMappingService.RetrieveProductInformationMap(connectorId, null, false);
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

            availableRegistrations = await _OAuthService.GetUserRegistrations();

            if (connectorId == "47")
                selectedRegistration = availableRegistrations?.Find(x => x.AppCode == connectorModel.DataSource.AppCode && x.Domain.Contains(dataBaseId));
            else
                selectedRegistration = availableRegistrations?.Find(x => x.AppCode == connectorModel.DataSource.AppCode);

            if (selectedRegistration != null)
            {
                var token = await _dataMapping.RetrieveToken(connectorModel.DataSource.AppCode, domain: selectedRegistration.Domain);
                // This function is here just to obtain the customer_id for the UrlPath for iManage connector
                Dictionary<string, string> headerParameters = await _dataExtractionService.ExtractDataSource(null, parameters, null, connectorDocument);
                if (!string.IsNullOrEmpty(token?.AccessToken))
                {
                    ApiInformation apiInfo = new ApiInformation()
                    {
                        AppCode = connectorDocument.DataSource.appCode,
                        HostName = string.IsNullOrEmpty(connectorDocument.HostName) ? (connectorDocument.DataSource.domain ?? selectedRegistration.Domain) : connectorDocument.HostName,
                        UrlPath = connectorDocument.Request.EndpointPath,
                        AuthHeader = connectorDocument.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader,
                        Token = token,
                        Method = "get", // TODO: We have to overwrite the method in here until we have a connector definition for each connector that supports document download
                        Headers = headerParameters
                    };

                    CleanUpApiInformation(apiInfo);

                    // TODO: If we want to add this feature for other connectors we should create a new connector definition for each data source that will
                    // support document download and have the urlpath defined there
                    switch (connectorId)
                    {
                        // iManage
                        case "44":
                            apiInfo.UrlPath = apiInfo.UrlPath.Replace("/documents", $"/libraries/{dataBaseId}/documents/{docId}/download");
                            break;
                        // MsGraph - SharePoint
                        case "14":
                            apiInfo.UrlPath = apiInfo.UrlPath.Replace("/search/microsoft.graph.query", $"/drives/{dataBaseId}/items/{docId}/content");
                            break;
                        // MsGraph - OneDrive
                        case "80":
                            apiInfo.UrlPath = apiInfo.UrlPath.Replace("/search/microsoft.graph.query", $"/me/drive/items/{docId}/content");
                            break;
                        // HighQ
                        case "47":
                            apiInfo.UrlPath = apiInfo.UrlPath.Replace("/api/12/search", $"/api/18/files/{docId}/content");
                            break;
                    }
                    
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
