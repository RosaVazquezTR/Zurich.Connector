using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.IntegrationTests
{

    /// <summary>
    /// Not really integration tests but will pull the json files and verify things look appropriate.
    /// </summary>
    public class ConnectorCosmosTests : IntegrationTest
    {
        private const string folderLocation = @"..\..\..\..\..\Zurich.Connector.Deploy\CosmosRecords";
        private List<string> dataSourceTypes = new List<string>() { "oauth2", "transferToken" };
        private List<string> parameterTypes = new List<string>() { "array", "date", "int", "object", "short", "string" };
        private List<string> requestMethodTypes = new List<string>() { "GET", "POST" };
        private List<string> requestInClauseTypes = new List<string>() { "Body", "OData", "Query" };

        public ConnectorCosmosTests(CustomWebApplicationFactory fixture) : base(fixture)
        {
        }

        public static List<DataSourceDocument> GetDataSources()
        {
            List<DataSourceDocument> dataSources = new List<DataSourceDocument>();
            string[] fileEntries = Directory.GetFiles($"{folderLocation}\\datasource");

            foreach (var fileLocation in fileEntries)
            {
                DataSourceDocument dataSource = null;
                using (StreamReader r = new StreamReader(fileLocation))
                {
                    string json = r.ReadToEnd();
                    dataSource = JsonConvert.DeserializeObject<DataSourceDocument>(json);
                }
                if (dataSource != null)
                {
                    dataSources.Add(dataSource);
                }
            }
            return dataSources;
        }

        public static List<ConnectorDocument> GetConnectors(string entityType, string subType)
        {
            List<ConnectorDocument> connectors = new List<ConnectorDocument>();
            string[] fileEntries = Directory.GetFiles($"{folderLocation}\\connector");

            foreach (var fileLocation in fileEntries)
            {
                ConnectorDocument connector = null;
                using (StreamReader r = new StreamReader(fileLocation))
                {
                    string json = r.ReadToEnd();
                    connector = JsonConvert.DeserializeObject<ConnectorDocument>(json);
                }
                if (connector != null)
                {
                    if (string.IsNullOrEmpty(entityType) || connector.info.entityType.ToString() == entityType)
                    {
                        if (string.IsNullOrEmpty(subType) || string.IsNullOrEmpty(connector.info.subType) || connector.info.subType == subType)
                        {
                            connectors.Add(connector);
                        }
                    }
                }
            }
            return connectors;
        }

        public static IEnumerable<object[]> GetDataSourcesTestCases()
        {
            var dataSources = GetDataSources();
            IEnumerable<object[]> testCases = dataSources.Select(x => new object[] { x });
            return testCases;
        }
        public static IEnumerable<object[]> GetConnectorsTestCases(string entityType)
        {
            var connectors = GetConnectors(entityType, string.Empty);
            IEnumerable<object[]> testCases = connectors.Select(x => new object[] { x });
            return testCases;
        }

        public static IEnumerable<object[]> GetConnectorsAndDataSourcesTestCases()
        {
            var dataSources = GetDataSources();
            var connectors = GetConnectors(string.Empty, string.Empty);

            var testCases = from connector in connectors
                            join dataSource in dataSources
                            on connector.info.dataSourceId equals dataSource.Id
                            select new object[] { connector, dataSource };

            return testCases;
        }

        /// <summary>
        /// Will check specific things related to just the datasource
        /// </summary>
        [Theory]
        [MemberData(nameof(GetDataSourcesTestCases))]
        public async Task VerifyDataSources(DataSourceDocument dataSource)
        {
            // Assert
            dataSource.Should().NotBeNull();
            
            dataSource.Id.Should().NotBeNullOrWhiteSpace();
            dataSource.partitionkey.Should().Be("DataSourceList");
            
            dataSource.appCode.Should().NotBeNullOrWhiteSpace();
            dataSource.description.Should().NotBeNullOrWhiteSpace();
            dataSource.name.Should().NotBeNullOrWhiteSpace();

            dataSource.securityDefinition.Should().NotBeNull();
            dataSource.securityDefinition.type.Should().ContainAny(dataSourceTypes);
        }

        /// <summary>
        /// Will check specific things related to just the connectors
        /// </summary>
        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: "")]
        public async Task VerifyConnectors(ConnectorDocument connector)
        {
            // Assert
            connector.Should().NotBeNull();

            connector.Id.Should().NotBeNull();
            connector.partitionkey.Should().Be("ConnectorList");
            connector.info.Should().NotBeNull();
            connector.info.title.Should().NotBeNullOrWhiteSpace();
            connector.info.description.Should().NotBeNullOrWhiteSpace();
            connector.info.entityType.ToString().Should().NotBeNullOrWhiteSpace();
            connector.info.dataSourceId.Should().NotBeNullOrWhiteSpace();
            connector.info.version.Should().NotBeNull();
            if (string.IsNullOrEmpty(connector.info.subType) || connector.info.subType.Equals("Parent"))
            {
                connector.Alias.Should().NotBeNull();
                connector.request.Should().NotBeNull();
                connector.request.endpointPath.Should().NotBeNull();
                connector.request.method.Should().ContainAny(requestMethodTypes);
                foreach (var param in connector.request.parameters)
                {
                    param.cdmname.Should().NotBeNullOrWhiteSpace();
                    param.name.Should().NotBeNullOrWhiteSpace();
                    param.inClause.Should().ContainAny(requestInClauseTypes);
                    param.type.Should().ContainAny(parameterTypes);
                }
                foreach (var sortParam in connector.request.sorting?.properties)
                {
                    sortParam.name.Should().NotBeNullOrWhiteSpace();
                    sortParam.element.Should().NotBeNull();
                    sortParam.elementValue.Should().NotBeNull();
                    sortParam.type.Should().ContainAny(parameterTypes);
                }
                if (connector.request.responseContentType == Data.Model.ResponseContentType.XML)
                {
                    connector.request.xmlArrayAttribute.Should().NotBeNull();
                }
                foreach (var filterValue in connector.filters)
                {
                    filterValue.Name.Should().NotBeNullOrWhiteSpace();
                    filterValue.Description.Should().NotBeNullOrWhiteSpace();
                    filterValue.RequestParameter.Should().NotBeNull();
                    foreach(var filter in filterValue.FilterList)
                    {
                        filter.Name.Should().NotBeNullOrWhiteSpace();
                        filter.Id.Should().NotBeNullOrWhiteSpace();
                    }
                }
            }
            else
            {
                connector.Alias.Should().BeNull();
                connector.request.endpointPath.Should().BeNull();
                connector.request.method.Should().BeNull();
                connector.request.parameters.Should().BeNull();
                connector.request.responseContentType.Should().BeNull();
                connector.request.sorting.Should().BeNull();
                connector.request.xmlArrayAttribute.Should().BeNull();
                connector.response.schema.Should().BeNull();
                connector.filters.Should().BeEmpty();
            }

            connector.cdmMapping.Should().NotBeNull();
            connector.cdmMapping.structured.Should().NotBeNull();
            foreach (var param in connector.cdmMapping.structured)
            {
                param.name.Should().NotBeNull();
                param.type.Should().ContainAny(parameterTypes);
                param.responseElement.Should().NotBeNull();

                // Verify child connector exists
                if (param.type == "object")
                {
                    var allEntityChildrenRecords = GetConnectors(connector.info.entityType.ToString(), "Child");
                    var match = Regex.Match(param.responseElement, @"{(.*?)}");
                    var connectionId = match.Groups[1].ToString();
                    var childConnector = allEntityChildrenRecords.SingleOrDefault(x => x.Id == connectionId);
                    childConnector.Should().NotBeNull();
                }
            }

        }

        /// <summary>
        /// Will check specific things related to the datasource and the connector
        /// </summary>
        [Theory]
        [MemberData(nameof(GetConnectorsAndDataSourcesTestCases))]
        public async Task VerifyConnectorsAndDataSources(ConnectorDocument connector, DataSourceDocument dataSource)
        {
            dataSource.Should().NotBeNull();
            // Assert
            if (string.IsNullOrEmpty(connector.info.subType) || connector.info.subType.Equals("Parent"))
            {
                connector.Alias.Should().StartWith($"{dataSource.appCode}.{connector.info.entityType}".ToLower());
            }

        }
    }
}
