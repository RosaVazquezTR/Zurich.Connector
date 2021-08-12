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
        private List<string> parameterTypes = new List<string>() { "array", "date", "int", "string" };

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

        public static IEnumerable<object[]> GetConnectorsAndDataSources()
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
            Assert.NotNull(dataSource);

            Assert.NotNull(dataSource.Id);
            Assert.Equal("DataSourceList", dataSource.partitionkey);
            Assert.NotNull(dataSource.appCode);
            Assert.NotNull(dataSource.description);
            Assert.NotNull(dataSource.name);

            Assert.NotNull(dataSource.securityDefinition);
            // Note the below contains is a tad strange because the expected is actually the actual
            Assert.Contains(dataSource.securityDefinition.type, dataSourceTypes);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: "")]
        public async Task VerifyConnectors(ConnectorDocument connector)
        {
            // Assert
            Assert.NotNull(connector);

            Assert.NotNull(connector.Id);
            Assert.Equal("ConnectorList", connector.partitionkey);
            Assert.NotNull(connector.info);
            Assert.NotNull(connector.info.title);
            Assert.NotNull(connector.info.description);
            Assert.NotNull(connector.info.entityType.ToString());
            Assert.NotNull(connector.info.dataSourceId);
            Assert.NotNull(connector.info.version);
            if (string.IsNullOrEmpty(connector.info.subType) || connector.info.subType.Equals("Parent"))
            {
                Assert.NotNull(connector.Alias);
            }
            else
            {
                Assert.Null(connector.request);
                Assert.Null(connector.response);
                Assert.Null(connector.filters);
            }

            Assert.NotNull(connector.cdmMapping);
            Assert.NotNull(connector.cdmMapping.structured);
            foreach(var param in connector.cdmMapping.structured)
            {
                Assert.NotNull(param.name);
                // Note the below contains is a tad strange because the expected is actually the actual
                Assert.Contains(param.type, parameterTypes);
                Assert.NotNull(param.responseElement);
            }

        }

        [Theory]
        [MemberData(nameof(GetConnectorsAndDataSources))]
        public async Task VerifyConnectorsAndDataSources(ConnectorDocument connector, DataSourceDocument dataSource)
        {
            // Assert
            if (string.IsNullOrEmpty(connector.info.subType) || connector.info.subType.Equals("Parent"))
            {
                Assert.StartsWith($"{dataSource.appCode}.{connector.info.entityType}".ToLower(), connector.Alias);
            }

        }
    }
}
