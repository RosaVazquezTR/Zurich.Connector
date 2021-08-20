using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.App.Services.DataSources
{
    public class IManageConnectorOperations : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;

        ///		/customers/{customerId}/libraries/{libraryId}/documents/{docId}/download/{fileName}
        ///		htts://2fdb2-dmobility.imanage.work/work/web/api/v2/customers/1/libraries/ACTIVE/documents/ACTIVE!119.1/download/CommonDataModel.xlsx?activity=export
        #region Endpoints
        private const string DocumentsEndpoint = "work/link/d/";
        private const string DocumentsDowloadEndpoint = "work/web/";
        private string customerId;
        #endregion

        public IManageConnectorOperations(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory )
        {
            _logger = logger;
            _dataMapping = dataMappingFactory.GetMapper(AuthType.OAuth2);
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.iManage;
        }

        public async Task<dynamic> SetItemLink(EntityType entityType, dynamic item, string hostName)
        {
            try
            {
                switch (entityType)
                {

                    case EntityType.Document:
                        if (item is JObject result && result.ContainsKey("Items"))
                        {
                            if (result["Items"] is JArray)
                            {
                                // 1 = iManage user profile
                                var dataTypeInformation = await _dataMapping.RetrieveProductInformationMap("1", hostName);
                                // Make api call to get the information for the url variable inside { }
                                JToken userProfileResponse = await _dataMapping.Get<JToken>(dataTypeInformation, string.Empty);
                                customerId = userProfileResponse["customer_id"].Value<string>();

                                foreach (JObject doc in (result["Items"] as JArray))
                                {
                                    if (!doc.ContainsKey(StructuredCDMProperties.WebUrl))
                                        doc[StructuredCDMProperties.WebUrl] = BuildLink(entityType, doc, hostName);
                                   if (!doc.ContainsKey(StructuredCDMProperties.DownloadUrl))
                                        doc[StructuredCDMProperties.DownloadUrl] = await BuildDownloadLink(entityType, doc, hostName);

                                }
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

        /// <summary>
        /// Downloads a document to an item in its data source app
        /// </summary>
        /// <param name="itemType">The data type</param>
        /// <param name="item">The target item</param>
        /// <param name="hostName">The data source host name</param>
        /// <returns></returns>
        /// ///		/customers/{customerId}/libraries/{libraryId}/documents/{docId}/download/{fileName}
        ///		htts://2fdb2-dmobility.imanage.work/work/web/api/v2/customers/1/libraries/ACTIVE/documents/ACTIVE!119.1/download/CommonDataModel.xlsx?activity=export
        private async Task<string> BuildDownloadLink(EntityType itemType, JObject item, string hostName)
        {
            string result = null;

            switch (itemType)
            {
                case EntityType.Document:
                    
               
                   

                    // Replace the url variable inside { }
                   // string value = result.Value<string>();
                    ///		/customers/{customerId}/libraries/{libraryId}/documents/{docId}/download/{fileName}
                   // var customerId = item.ContainsKey(UnstructuredCDMProperties.UserId) ? item[UnstructuredCDMProperties.UserId].Value<string>() : "";
                    var libraryId = item.ContainsKey(UnstructuredCDMProperties.Library) ? item[UnstructuredCDMProperties.Library].Value<string>() : "";
                    var docId = item.ContainsKey(StructuredCDMProperties.EntityId) ? item[StructuredCDMProperties.EntityId].Value<string>() : "";
                    var builder = new UriBuilder("https", hostName, -1);
                    result = $"{builder.Uri}{DocumentsDowloadEndpoint}/customers/{customerId}/libraries{libraryId}/documents{docId}/download"; 
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
        private string BuildLink(EntityType itemType, JObject item, string hostName)
        {
            string result = null;
            switch (itemType)
            {
                case EntityType.Document:
                    var docId = item.ContainsKey(StructuredCDMProperties.EntityId) ? item[StructuredCDMProperties.EntityId].Value<string>() : "";
                    var builder = new UriBuilder("https", hostName, -1);
                    result = $"{builder.Uri}{DocumentsEndpoint}{docId}"; //"http//2fdb2-dmobility.imanage.work/work/link/d/Active!119.1"
                    break;
            }
            return result;
        }
    }
}
