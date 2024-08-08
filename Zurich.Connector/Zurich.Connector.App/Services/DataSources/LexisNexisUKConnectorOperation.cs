using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Model;
using static Zurich.Connector.Data.Constants;

namespace Zurich.Connector.App.Services.DataSources
{
    /// <summary>
    /// The LexisNexisUKConnectorOperation class is responsible for handling operations related specifically to the LexisNexis UK connector.
    /// At this stage, it handles the creation of a valid url for documents in the search result.
    /// </summary>
    public class LexisNexisUKConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;

        public LexisNexisUKConnectorOperation(ILogger<IConnectorDataSourceOperations> logger)
        {
            _logger = logger;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.lexisNexisUK;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            try
            {
                switch (entityType)
                {
                    case ConnectorEntityType.Search:
                        if (item is JObject searchResult && searchResult.ContainsKey(PropertiesConnectorDefinition.Count))
                        {
                            if (searchResult.ContainsKey(PropertiesConnectorDefinition.Documents) && searchResult[PropertiesConnectorDefinition.Documents].HasValues)
                            {
                                var documents = searchResult[PropertiesConnectorDefinition.Documents] as JArray;
                                searchResult[StructuredCDMProperties.ItemsCount] = (short)documents.Count;
                                foreach (JObject doc in documents)
                                {
                                    if (doc.ContainsKey(StructuredCDMProperties.WebUrl))
                                    {
                                        JObject additionalProperties = (JObject)doc[StructuredCDMProperties.AdditionalProperties];
                                        doc[StructuredCDMProperties.WebUrl] = BuildLink(entityType, doc);
                                    }
                                }
                            }
                        }
                        break;
                    case ConnectorEntityType.Document:
                        // To add the logic here when we enable Lexis document download
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
        /// Generates a link to an item in its data source app
        /// </summary>
        /// <param name="itemType">The data type</param>
        /// <param name="item">The target item</param>
        /// <returns>A valid url to view the document in the data source app</returns>
        private string BuildLink(ConnectorEntityType itemType, JObject item)
        {
            string documentUrl = null;

            switch (itemType)
            {
                case ConnectorEntityType.Document:
                    // To add the logic here when we enable Lexis document download
                    break;
                case ConnectorEntityType.Search:
                    string pddocfullpath;
                    string path = (string)item["WebUrl"];
                    string pattern = @"DocumentId='(.*?)'";

                    Match match = Regex.Match(path, pattern);

                    if (match.Success)
                        pddocfullpath = Uri.UnescapeDataString(match.Groups[1].Value);
                    else
                        pddocfullpath = path;

                    documentUrl = DataConstants.lexisDocumentsBaseUrl + "?pddocfullpath=" + pddocfullpath;
                    break;
            }
            return documentUrl;

        }

    }
}

