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
using Zurich.Connector.App.Factories;
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
        Task<string> GetDocumentContentAsync(string connectorId, string docId, bool transformToPDF = true);
    }

    public class DocumentDownloadService(IOAuthServices OAuthService, IDataMappingFactory dataMappingFactory, IDataMappingService dataMappingService, IRedisRepository redisRepository, OAuthOptions oAuthOptions, IRepository repository, IDataExtractionService dataExtractionService, IMapper mapper) : IDocumentDownloadService
    {
        private readonly IDataMapping _dataMapping;

        public async Task<string> GetDocumentContentAsync(string connectorId, string docId, bool transformToPDF = true)
        {
            (string dataBaseId, string documentId) = ParseDocumentId(connectorId, docId);

            //string document = await redisRepository.GetAsync<string>(documentId);

            //if (string.IsNullOrEmpty(document))
            //{
                ConnectorModel connectorModel = await GetConnectorModelAsync(connectorId);
                ConnectorModel downloadConnectorModel = await GetConnectorModelAsync(connectorModel.Info.DownloadConnector);
                ConnectorDocument connectorDocument = mapper.Map<ConnectorDocument>(downloadConnectorModel);
                DataSourceInformation selectedRegistration;

                if (downloadConnectorModel.DataSource.Id != connectorModel.DataSource.Id)
                {
                    selectedRegistration = await GetSelectedRegistrationAsync(downloadConnectorModel.DataSource.AppCode, dataBaseId);
                    connectorModel = downloadConnectorModel;
                }
                else
                {
                    selectedRegistration = await GetSelectedRegistrationAsync(connectorModel.DataSource.AppCode, dataBaseId);
                }

                IDataMapping service = dataMappingFactory.GetImplementation(connectorModel?.DataSource?.SecurityDefinition?.Type);

                Dictionary<string, string> headerParameters = await dataExtractionService.ExtractDataSource(null, new Dictionary<string, string>
                {
                    { "dataBaseId", dataBaseId },
                    { "docId", documentId }
                }, null, connectorDocument);

                var data = await service.GetAndMapResults<dynamic>(connectorDocument, null, null, headerParameters, null);
                //await redisRepository.SetAsync(documentId, document);
            //}

            return data;
        }

        private async Task<ConnectorModel> GetConnectorModelAsync(string connectorId)
        {
            return await dataMappingService.RetrieveProductInformationMap(connectorId, null, false);
        }

        private static (string, string) ParseDocumentId(string connectorId, string docId)
        {
            string databaseId = string.Empty;
            string documentId = docId;

            switch (connectorId)
            {
                case "44":
                    databaseId = docId.Split('!')[0];
                    break;

                case "14":
                case "80":
                case "47":
                    var parts = docId.Split(',');
                    databaseId = parts[0];
                    documentId = parts.Length > 1 ? parts[1] : string.Empty;
                    break;
            }

            return (databaseId, documentId);
        }

        private async Task<DataSourceInformation> GetSelectedRegistrationAsync(string connectorId, string dataBaseId)
        {
            List<DataSourceInformation> availableRegistrations = await OAuthService.GetUserRegistrations();

            return connectorId == "47"
                ? availableRegistrations?.Find(x => x.AppCode == connectorId && x.Domain.Contains(dataBaseId))
                : availableRegistrations?.Find(x => x.AppCode == connectorId);
        }

        private ApiInformation CreateApiInformation(ConnectorDocument connectorDocument, DataSourceInformation selectedRegistration, OAuthAPITokenResponse token, Dictionary<string, string> headerParameters)
        {
            ApiInformation apiInfo = new()
            {
                AppCode = connectorDocument.DataSource.appCode,
                HostName = CleanUpHostName(connectorDocument.HostName, selectedRegistration.Domain, connectorDocument.DataSource.appCode),
                UrlPath = connectorDocument.Request.EndpointPath,
                AuthHeader = connectorDocument.DataSource.securityDefinition.defaultSecurityDefinition.authorizationHeader,
                Token = token,
                Method = connectorDocument.Request.Method,
                Headers = headerParameters
            };

            return apiInfo;
        }

        private string CleanUpHostName(string hostName, string domain, string appCode)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                if (oAuthOptions.Connections.TryGetValue(appCode, out OAuthConnection connection))
                {
                    return UrlUtils.FormattingUrl(connection.BaseUrl);
                }
                else
                {
                    return domain;
                }
            }
            else
            {
                return UrlUtils.FormattingUrl(hostName);
            }
        }
    }
}
