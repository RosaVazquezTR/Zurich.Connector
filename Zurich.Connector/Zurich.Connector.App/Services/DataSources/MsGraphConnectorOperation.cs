using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    public class MsGraphConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;
        private readonly IDataMapping _dataMapping;
        private readonly IConfiguration _configuration;

        public MsGraphConnectorOperation(ILogger<IConnectorDataSourceOperations> logger, IDataMappingFactory dataMappingFactory, IConfiguration configuration)
        {
            _logger = logger;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _configuration = configuration;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.msGraph;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Search:
                        if (item is JObject result && result.ContainsKey("Documents"))
                        {
                            foreach (JObject doc in (result["Documents"] as JArray))
                            {
                                if (doc.ContainsKey(StructuredCDMProperties.Title))
                                {
                                    if (doc.ContainsKey(StructuredCDMProperties.Type))
                                    {
                                        doc[StructuredCDMProperties.Type] = GetExtension(doc[StructuredCDMProperties.Title].ToString());
                                    }
                                    doc[StructuredCDMProperties.Title] = RemoveExtension(doc[StructuredCDMProperties.Title].ToString());
                                }
                                if (doc.ContainsKey(StructuredCDMProperties.Snippet))
                                {
                                    doc[StructuredCDMProperties.Snippet] = UpdateSnippet(doc[StructuredCDMProperties.Snippet].ToString());
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
    }
}
