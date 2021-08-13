using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.IntegrationTests
{


    public class ConnectorCosmosTests : IntegrationTest
    {
        private const string folderLocation = @"..\..\..\..\..\Zurich.Connector.Deploy\CosmosRecords";
        private List<string> dataSourceTypes = new List<string>() { "oauth2", "transferToken" };
        private List<string> parameterTypes = new List<string>() { "array", "date", "int", "string", "short" };

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

        public static List<ConnectorDocument> GetConnectors(string entityType)
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
                        connectors.Add(connector);
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
            var connectors = GetConnectors(entityType);
            IEnumerable<object[]> testCases = connectors.Select(x => new object[] { x });
            return testCases;
        }

        public static IEnumerable<object[]> GetConnectorsAndDataSourcesTestCases()
        {
            var dataSources = GetDataSources();
            var connectors = GetConnectors(string.Empty);

            var testCases = from connector in connectors
                            join dataSource in dataSources
                            on connector.dataSource.Id equals dataSource.Id
                            select new object[] { connector, dataSource };

            return testCases;
        }

        [Theory]
        [MemberData(nameof(GetDataSourcesTestCases))]
        public async Task VerifyDataSources(DataSourceDocument dataSource)
        {
            // Assert
            dataSource.Should().NotBeNull();
            
            dataSource.Id.Should().NotBeNull();
            dataSource.partitionkey.Should().Be("DataSourceList");
            
            dataSource.appCode.Should().NotBeNull();
            dataSource.description.Should().NotBeNull();
            dataSource.name.Should().NotBeNull();

            dataSource.securityDefinition.Should().NotBeNull();
            dataSource.securityDefinition.type.Should().ContainAny(dataSourceTypes);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: "")]
        public async Task VerifyConnectors(ConnectorDocument connector)
        {
            // Assert
            connector.Should().NotBeNull();

            connector.Id.Should().NotBeNull();
            connector.partitionkey.Should().Be("ConnectorList");
            connector.info.Should().NotBeNull();
            connector.info.title.Should().NotBeNull();
            connector.info.description.Should().NotBeNull();
            connector.info.entityType.ToString().Should().NotBeNull();
            connector.info.dataSourceId.Should().NotBeNull();
            connector.info.version.Should().NotBeNull();
            if (string.IsNullOrEmpty(connector.info.subType) || connector.info.subType.Equals("Parent"))
            {
                connector.Alias.Should().NotBeNull();
            }
            else
            {
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
                // Note the below contains is a tad strange because the expected is actually the actual
                //Assert.Contains(param.type, parameterTypes);
                param.type.Should().ContainAny(parameterTypes);
                param.responseElement.Should().NotBeNull();
            }

        }

        [Theory]
        [MemberData(nameof(GetConnectorsAndDataSourcesTestCases))]
        public async Task VerifyConnectorsAndDataSources(ConnectorDocument connector, DataSourceDocument dataSource)
        {
            // Assert
            if (string.IsNullOrEmpty(connector.info.subType) || connector.info.subType.Equals("Parent"))
            {
                connector.Alias.Should().StartWith($"{dataSource.appCode}.{connector.info.entityType}".ToLower());
            }

        }
    }
}
