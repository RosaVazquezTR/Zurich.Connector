using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    public class Microsoft365ConnectorOperations : IConnectorDataSourceOperations
    {
        // always use ReferenceTypeFilter to get only drive items, so we don't get sharepoint sites
        public const string ReferenceTypeFilter = "resourceReference/type eq 'microsoft.graph.driveItem'";
        public const string VisualizationTypeFilterTemplate = "ResourceVisualization/Type eq '<type>'";

        private readonly ILogger _logger;

        public Microsoft365ConnectorOperations(ILogger<IConnectorDataSourceOperations> logger)
        {
            _logger = logger;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.MsGraph;
        }

        public dynamic SetItemLink(EntityType entityType, dynamic item, string hostName)
        {
            return item;
        }

        public NameValueCollection UpdateQueryParams(EntityType entityType, NameValueCollection queryParams)
        {
            try
            {
                switch (entityType)
                {
                    case EntityType.Document:
                        string typeFilters = queryParams.Get("$filters");
                        if (!String.IsNullOrEmpty(typeFilters))
                        {
                            var resourceVisualizationTypes = typeFilters.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            var terms = resourceVisualizationTypes
                                .Select(t => VisualizationTypeFilterTemplate.Replace("<type>", t));

                            var visualizationTypeFilter = string.Join(" or ", terms);

                            var filter = $"{ReferenceTypeFilter} and ({visualizationTypeFilter})";
                            queryParams.Set("$filters", filter);
                        } 
                        else
                        {
                            queryParams.Set("$filters", ReferenceTypeFilter);
                        }
                        break;
                }
            }
            catch (UriFormatException ex)
            {
                _logger.LogError("Unable to parse {entityType} web URL: {message}", entityType.ToString(), ex.Message ?? "");
            }
            return queryParams;
        }
    }
}
