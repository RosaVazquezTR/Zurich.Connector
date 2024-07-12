using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Interfaces;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Utils;

namespace Zurich.Connector.App.Services
{
    /// <summary>
    /// Interface for managing document download services.
    /// </summary>
    public interface IDocumentDownloadService
    {
        /// <summary>
        /// Asynchronously gets the content of a document based on a download request model.
        /// </summary>
        /// <param name="downloadRequestModel">The model containing the details of the download request, including connector ID and document ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the document content as a stream.</returns>
        Task<Stream> GetDocumentContentAsync(DocumentDownloadRequestModel downloadRequestModel);

        /// <summary>
        /// Asynchronously gets the content of a document as a string. Optionally, the document can be transformed to PDF before getting its content.
        /// </summary>
        /// <param name="documentStream">The stream containing the document's data.</param>
        /// <param name="transformToPdf">A flag indicating whether the document should be converted to PDF. Defaults to true.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the document content as a string.</returns>
        Task<string> GetDocumentContentAsStringAsync(Stream documentStream, bool transformToPdf = true);
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

        public async Task<Stream> GetDocumentContentAsync(DocumentDownloadRequestModel downloadRequestModel)
        {
            (string dataBaseId, string documentId) = ParseDocumentId(downloadRequestModel.ConnectorId, downloadRequestModel.DocId);

            ConnectorModel connectorModel = await GetConnectorModelAsync(downloadRequestModel.ConnectorId);
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

            string result = await service.GetAndMapResults<dynamic>(connectorDocument, null, null, headerParameters, null);

            if (string.IsNullOrEmpty(result))
            {
                throw new ApplicationException("Document content is empty.");
            }

            return new MemoryStream(Convert.FromBase64String(result));
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

        public async Task<string> GetDocumentContentAsStringAsync(Stream documentStream, bool transformToPdf = true)
        {
            return await repository.HandleSuccessResponse(documentStream, transformToPdf);
        }
    }
}
