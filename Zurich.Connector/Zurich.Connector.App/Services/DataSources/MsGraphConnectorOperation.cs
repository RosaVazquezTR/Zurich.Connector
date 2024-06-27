using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App.Factories;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.App.Services.DataSources
{
    public class MsGraphConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;
        private readonly IConfiguration _configuration;
        private readonly ICosmosService _cosmosService;
        private readonly IMapper _mapper;

        public MsGraphConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IConfiguration configuration, ICosmosService cosmosService, IMapper mapper)
        {
            _logger = logger;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _configuration = configuration;
            _cosmosService = cosmosService;
            _mapper = mapper;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode.StartsWith(KnownDataSources.msGraph);
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Search:
                        if (item is JObject result && result.ContainsKey("Documents") && result["Documents"].HasValues)
                        {
                            foreach (JObject doc in (result["Documents"] as JArray))
                            {
                                if (doc.ContainsKey(StructuredCDMProperties.Title))
                                {
                                    if (doc.ContainsKey(StructuredCDMProperties.Type))
                                    {
                                        var extension = GetExtension(doc[StructuredCDMProperties.Title].ToString());
                                        doc[StructuredCDMProperties.Type] = ConnectorOperationsUtility.MapExtensionToDocumentType(extension);
                                        doc[StructuredCDMProperties.AdditionalProperties][UnstructuredCDMProperties.Extension] = extension;
                                    }
                                    doc[StructuredCDMProperties.Title] = RemoveExtension(doc[StructuredCDMProperties.Title].ToString());
                                }
                                if (doc.ContainsKey(StructuredCDMProperties.Snippet))
                                {
                                    doc[StructuredCDMProperties.Snippet] = UpdateSnippet(doc[StructuredCDMProperties.Snippet].ToString());
                                }
                                if (doc.ContainsKey(StructuredCDMProperties.WebUrl))
                                {
                                    doc[StructuredCDMProperties.WebUrl] = UpdateWebUrl(doc);
                                }
                            }
                        }
                        else if (item is string)
                        {
                            item = await GetOneDriveUrl(appCode, hostName);
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

        public async Task<Dictionary<string, string>> SetParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            return allParameters;
        }
        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            return item;
        }

        /// <summary>
        /// Pulls the extention form the title so can be returned on type
        /// </summary>
        /// <param name="title">Title from MsGrapoh</param>
        /// <returns>extension</returns>
        private string GetExtension(string title)
        {
            string extension = string.Empty;

            var splitString = title.Split(".");
            if (splitString.Count() > 1)
            {
                extension = splitString.Last();
            }

            return extension;
        }

        /// <summary>
        /// Removes the extension from the title
        /// </summary>
        /// <param name="title">Original title from msGraph</param>
        /// <returns>Title with no extension</returns>
        private string RemoveExtension(string title)
        {
            var splitString = title.Split(".");
            if (splitString.Count() > 1)
            {
                title = string.Join(".", splitString.SkipLast(1));
            }
            
            return title;
        }

        /// <summary>
        /// Updates the snippet to replace c0 tags with bolds and remove ddd/>
        /// </summary>
        /// <param name="snippet">Snippet from MsGraph</param>
        /// <returns>Snippet with updated tags</returns>
        internal static string UpdateSnippet(string snippet)
        {
            if(!string.IsNullOrEmpty(snippet))
            {
                snippet = snippet.Replace("<c0>", "<b>").Replace("</c0>", "</b>").Replace(" <ddd/>", ". ");
            }
            return snippet;
        }

        internal static string UpdateWebUrl(JObject doc)
        {
            string returnUrl = doc[StructuredCDMProperties.WebUrl].Value<string>();
            JObject additionalProperties = (JObject)doc[StructuredCDMProperties.AdditionalProperties];

            if (doc.ContainsKey(StructuredCDMProperties.Title) && 
                additionalProperties.ContainsKey(UnstructuredCDMProperties.ListItemUniqueId) &&
                additionalProperties.ContainsKey(UnstructuredCDMProperties.Extension))
            {
                var extension = additionalProperties[UnstructuredCDMProperties.Extension].Value<string>();
                if (string.IsNullOrEmpty(extension) || extension.ToLower() == "pdf" || extension.ToLower() == "txt")
                    return returnUrl;

                string containingFolder = "";
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    containingFolder = returnUrl.Contains("/Documents/") ? "/Documents/" : (returnUrl.Contains("/Shared Documents/") ? "/Shared Documents/" : "");
                }
                var title = doc[StructuredCDMProperties.Title].Value<string>();
                var listItemUniqueId = additionalProperties[UnstructuredCDMProperties.ListItemUniqueId].Value<string>();

                returnUrl = (string.IsNullOrEmpty(returnUrl) || string.IsNullOrEmpty(containingFolder)) ? returnUrl :
                                            returnUrl.Substring(0, (returnUrl.IndexOf(containingFolder))) +
                                            $"/_layouts/15/Doc.aspx?sourcedoc=%7B{listItemUniqueId}%7D&file={title}.{extension}&action=default&mobileredirect=true&DefaultItemOpen=1";
            }
            return returnUrl;
        }

        /// <summary>
        /// Gets the user's OneDrive url from OneDrive's user profile
        /// </summary>
        /// <param name="appCode">The data source app code</param>
        /// <param name="hostName">The data source host name</param>
        private async Task<string> GetOneDriveUrl(string appCode, string hostName)
        {
            ConnectorModel connectorModel = null;
            if (appCode == KnownDataSources.oneDrive)
            {
                // 79 = OneDrive user info
                connectorModel = await _cosmosService.GetConnector("79", true);
            }
            
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);
            // Make api call to get the information for the webUrl variable
            connectorDocument.HostName = hostName;
            JToken userProfileResponse = await _dataMapping.GetAndMapResults<JToken>(connectorDocument, string.Empty, null, null, null);
            return userProfileResponse["webUrl"].Value<string>();
        }
    }
}
