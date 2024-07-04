using AutoMapper;
using System.Collections.Generic;
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
    /// Interface to manage the document download service
    /// </summary>
    public interface IDocumentDownloadService
    {
        /// <summary>
        /// Gets the content of a document
        /// </summary>
        /// <param name="connectorId">Connector id</param>
        /// <param name="transformToPDF">Flag that determines whether the document should be converted to PDF</param>
        /// <param name="docId">Document id</param>
        /// <returns>Document content as string</returns>
        Task<string> GetDocumentContentAsync(string connectorId, string docId, bool transformToPDF = true);
    }

    /// <summary>
    /// Service that handles the logic related to document downloading
    /// </summary>
    public class DocumentDownloadService(
        IOAuthServices OAuthService,
        IDataMappingFactory dataMappingFactory,
        IDataMappingService dataMappingService,
        IRedisRepository redisRepository,
        OAuthOptions oAuthOptions,
        IRepository repository,
        IDataExtractionService dataExtractionService,
        IMapper mapper) : IDocumentDownloadService
    {
        private readonly IDataMapping _dataMapping;

        public async Task<string> GetDocumentContentAsync(string connectorId, string docId, bool transformToPDF = true)
        {
            (string dataBaseId, string documentId) = ParseDocumentId(connectorId, docId);

            //string document = await redisRepository.GetAsync<string>(documentId);
            string document = string.Empty;

            //if (string.IsNullOrEmpty(document))
            //{
            ConnectorModel connectorModel = await GetConnectorModelAsync(connectorId);
            ConnectorModel downloadConnectorModel = await GetConnectorModelAsync(connectorModel.Info.DownloadConnector);
            ConnectorDocument connectorDocument = mapper.Map<ConnectorDocument>(downloadConnectorModel);

            if (downloadConnectorModel.DataSource.Id != connectorModel.DataSource.Id)
            {
                connectorModel = downloadConnectorModel;
            }


            IDataMapping service = dataMappingFactory.GetImplementation(connectorModel?.DataSource?.SecurityDefinition?.Type);

            Dictionary<string, string> headerParameters = await dataExtractionService.ExtractDataSource(null, new Dictionary<string, string>
                {
                    { "dataBaseId", dataBaseId },
                    { "docId", documentId }
                }, null, connectorDocument);

            var data = await service.GetAndMapResults<dynamic>(connectorDocument, null, null, headerParameters, null);

            if (data != null)
            {
                document = await repository.HandleSuccessResponse(data, transformToPDF);
            }
            //await redisRepository.SetAsync(documentId, document);
            //}

            return document;
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
