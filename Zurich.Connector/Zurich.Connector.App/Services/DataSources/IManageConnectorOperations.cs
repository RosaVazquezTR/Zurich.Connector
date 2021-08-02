using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    public class IManageConnectorOperations : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;

        #region Endpoints
        private const string DocumentsEndpoint = "work/link/d/";
        #endregion

        public IManageConnectorOperations(ILogger<IConnectorDataSourceOperations> logger)
        {
            _logger = logger;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.iManage;
        }

        public dynamic SetItemLink(EntityType entityType, dynamic item, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case EntityType.Document:
                        if (item is JArray)
                        {
                            foreach (JObject doc in (item as JArray))
                            {
                                if (!doc.ContainsKey(StructuredCDMProperties.WebUrl))
                                    doc[StructuredCDMProperties.WebUrl] = BuildLink(entityType, doc, hostName);
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
                    result = $"{builder.Uri}{DocumentsEndpoint}{docId}";
                break;
            }
            return result;
        }
    }
}
