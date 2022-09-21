using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.App.Services.DataSources
{
    public class IManageConnectorOperations : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;
        private readonly ICosmosService _cosmosService;
        private readonly IMapper _mapper;

        #region Endpoints
        private const string DocumentsWebEndpoint = "work/link/d/";
        private const string FoldersWebEndpoint = "work/link/f/";
        private const string DocumentsDowloadEndpoint = "work/web/api/v2";
        private string customerId;
        #endregion

        public IManageConnectorOperations(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IMapper mapper, ICosmosService cosmosService)
        {
            _logger = logger;
            _mapper = mapper;
            _cosmosService = cosmosService;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.iManage || appCode == KnownDataSources.iManageServiceApp;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Document:
                        if (item is JObject docResult && docResult.ContainsKey("Items") && docResult["Items"].HasValues && docResult["Items"] is JArray)
                        {
                            customerId = await GetCustomerId(appCode, hostName);
                            foreach (JObject doc in docResult["Items"] as JArray)
                            {
                                if (!doc.ContainsKey(StructuredCDMProperties.WebUrl))
                                    doc[StructuredCDMProperties.WebUrl] = BuildLink(entityType, doc, hostName);
                                if (!doc.ContainsKey(StructuredCDMProperties.DownloadUrl))
                                    doc[StructuredCDMProperties.DownloadUrl] = BuildDownloadLink(entityType, doc, hostName);
                            }
                        }
                        break;
                    case ConnectorEntityType.Folder:
                        if (item is JObject folderResult && folderResult.ContainsKey("Items") && folderResult["Items"].HasValues && folderResult["Items"] is JArray)
                        {
                            customerId = await GetCustomerId(appCode, hostName);
                            foreach (JObject folder in folderResult["Items"] as JArray)
                            {
                                if (!folder.ContainsKey(StructuredCDMProperties.WebUrl))
                                    folder[StructuredCDMProperties.WebUrl] = BuildLink(entityType, folder, hostName);
                            }
                        }
                        break;
                    case ConnectorEntityType.Search:
                        if (item is JObject searchResult && searchResult.ContainsKey("Count"))
                        {
                            if (searchResult.ContainsKey("Documents") && searchResult["Documents"].HasValues)
                            {
                                var documents = searchResult["Documents"] as JArray;
                                searchResult[StructuredCDMProperties.ItemsCount] = (short)documents.Count;
                                foreach (JObject doc in documents)
                                {
                                    if (doc.ContainsKey(StructuredCDMProperties.AdditionalProperties))
                                    {
                                        doc[StructuredCDMProperties.WebUrl] = BuildLink(entityType, doc, hostName);
                                        var extension = ((JObject)doc[StructuredCDMProperties.AdditionalProperties]).ContainsKey(UnstructuredCDMProperties.Extension) ? doc[StructuredCDMProperties.AdditionalProperties][UnstructuredCDMProperties.Extension].Value<string>() : "";
                                        doc[StructuredCDMProperties.Type] = ConnectorOperationsUtility.MapExtensionToDocumentType(extension);
                                    }
                                }
                            }
                            else
                            {
                                searchResult[StructuredCDMProperties.ItemsCount] = 0;
                            }
                        }
                        break;
                }
            }
            catch (UriFormatException ex)
            {
                _logger.LogError("Unable to parse {entityType} web URL: {message}", entityType.ToString(), ex.Message ?? "");
            }
            return item;
        }

        public async Task<Dictionary<string, string>> SetSortParameters(Dictionary<string, string> allParameters)
        {
            return allParameters;
        }

        /// <summary>
        /// Downloads a document to an item in its data source app
        /// </summary>
        /// <param name="itemType">The data type</param>
        /// <param name="item">The target item</param>
        /// <param name="hostName">The data source host name</param>
        /// <returns></returns>
        private string BuildDownloadLink(ConnectorEntityType itemType, JObject item, string hostName)
        {
            string result = null;

            switch (itemType)
            {
                case ConnectorEntityType.Document:
                    // Fetching LibraryId from DesktopUrl = "iwl:dms=2fdb2-dmobility.imanage.work&&lib=Active&&num=118&&ver=1",
                    string desktopUrl = item.ContainsKey(StructuredCDMProperties.DesktopUrl) ? item[StructuredCDMProperties.DesktopUrl].Value<string>() : "";
                    Uri uri = new Uri(desktopUrl);
                    var libraryId = HttpUtility.ParseQueryString(uri.AbsoluteUri).Get("lib");
                    var docId = item.ContainsKey(StructuredCDMProperties.EntityId) ? item[StructuredCDMProperties.EntityId].Value<string>() : "";
                    var builder = new UriBuilder("https", hostName, -1);
                    result = $"{builder.Uri}{DocumentsDowloadEndpoint}/customers/{customerId}/libraries/{libraryId}/documents/{docId}/download";
                    break;
            }
            return result;
        }

        /// <summary>
        /// Generates a link to an item in its data source app
        /// </summary>
        /// <param name="itemType">The data type</param>
        /// <param name="item">The target item</param>
        /// <param name="hostName">The data source host name</param>
        /// <returns></returns>
        private string BuildLink(ConnectorEntityType itemType, JObject item, string hostName)
        {
            string result = null;
            string id = null;
            string endpoint = null;

            switch (itemType)
            {
                case ConnectorEntityType.Document:
                    id = item.ContainsKey(StructuredCDMProperties.EntityId) ? item[StructuredCDMProperties.EntityId].Value<string>() : "";
                    endpoint = DocumentsWebEndpoint;
                    break;
                case ConnectorEntityType.Folder:
                    id = item.ContainsKey(StructuredCDMProperties.EntityId) ? item[StructuredCDMProperties.EntityId].Value<string>() : "";
                    endpoint = FoldersWebEndpoint;
                    break;
                case ConnectorEntityType.Search:
                    var additionalProperties = (JObject)item[StructuredCDMProperties.AdditionalProperties];
                    id = additionalProperties.ContainsKey(UnstructuredCDMProperties.Id) ? additionalProperties[UnstructuredCDMProperties.Id].Value<string>() : "";
                    endpoint = DocumentsWebEndpoint;
                    break;
            }

            if (!string.IsNullOrEmpty(id))
            {
                var builder = new UriBuilder("https", hostName, -1);
                result = $"{builder.Uri}{endpoint}{id}";
            }

            return result;
        }

        /// <summary>
        /// Gets the user's customer id from iManage's user profile
        /// </summary>
        /// <param name="appCode">The data source app code</param>
        /// <param name="hostName">The data source host name</param>
        private async Task<string> GetCustomerId(string appCode, string hostName)
        {
            ConnectorModel connectorModel = null;
            if (appCode == KnownDataSources.iManageServiceApp)
            {
                // 55 = iManageServiceApp user profile
                connectorModel = await _cosmosService.GetConnector("55", true);
            }
            else
            {
                // 1 = iManage user profile
                connectorModel = await _cosmosService.GetConnector("1", true);
            }

            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
            // Make api call to get the information for the url variable inside { }
            connectorDocument.HostName = hostName;
            JToken userProfileResponse = await _dataMapping.GetAndMapResults<JToken>(connectorDocument, string.Empty, null, null, null);
            return userProfileResponse["customer_id"].Value<string>();
        }
    }
}
