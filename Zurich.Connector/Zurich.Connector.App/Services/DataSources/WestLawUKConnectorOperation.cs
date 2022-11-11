using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Services.DataSources
{
    public class WestLawUKConnectorOperation : IConnectorDataSourceOperations
    {
        private readonly ILogger _logger;

        public WestLawUKConnectorOperation(ILogger<IConnectorDataSourceOperations> logger)
        {
            _logger = logger;
        }

        public bool IsCompatible(string appCode)
        {
            return appCode == KnownDataSources.westLawUK;
        }

        public async Task<dynamic> AddAditionalInformation(ConnectorModel connector, dynamic item)
        {
            return item;
        }

        public async Task<dynamic> SetItemLink(ConnectorEntityType entityType, dynamic item, string appCode, string hostName)
        {
            return item;
        }

        public async Task<Dictionary<string, string>> SetParametersSpecialCases(ConnectorModel connector, Dictionary<string, string> allParameters)
        {
            var content = allParameters["content"];
            var isRelevanceSortType = allParameters["sortType"].Equals("UK_RESEARCH_RELEVANCE");

            if (content.Equals("Home/WestlawUK") || content.StartsWith("Home/WestlawUK/CurrentAwareness") ||
                content.StartsWith("Home/WestlawUK/WLScotsLaw") || content.StartsWith("Home/WestlawUK/Topic"))
            {
                UpdateSortType(isRelevanceSortType, "UK_RESEARCH_RELEVANCE", "UK_RESEARCH_BY_PUBLISHED_DATE", allParameters);
            }
            else if (content.StartsWith("Home/WestlawUK/Cases") || content.Equals("Home/WestlawUK/Journals"))
            {
                UpdateSortType(isRelevanceSortType, "UK_RESEARCH_RELEVANCE", "UK_RESEARCH_DATE_DESC", allParameters);
            }
            else if (content.StartsWith("Home/WestlawUK/WLLegislation"))
            {
                UpdateSortType(isRelevanceSortType, "UK_RESEARCH_RELEVANCE", null, allParameters);
            }
            else
            {
                allParameters.Remove("sortType");
            }

            return allParameters;
        }

        private void UpdateSortType(bool isRelevanceSortType, string relevanceValue, string dateValue, Dictionary<string, string> allParameters)
        {
            if (isRelevanceSortType)
            {
                if (string.IsNullOrEmpty(relevanceValue))
                    allParameters.Remove("sortType");
                else
                    allParameters["sortType"] = relevanceValue;
            }
            else
            {
                if (string.IsNullOrEmpty(dateValue))
                    allParameters.Remove("sortType");
                else
                    allParameters["sortType"] =  dateValue;
            }
        }
    }
}
