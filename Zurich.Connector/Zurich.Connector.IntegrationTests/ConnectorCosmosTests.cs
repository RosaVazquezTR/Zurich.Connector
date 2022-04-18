using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Zurich.Connector.App;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.IntegrationTests
{

    /// <summary>
    /// Not really integration tests but will pull the json files and verify things look appropriate.
    /// </summary>
    public class ConnectorCosmosTests : IntegrationTest
    {
        private const string folderLocation = @"..\..\..\..\..\Zurich.Connector.Deploy\CosmosRecords";
        private List<string> dataSourceTypes = new List<string>() { "oauth2", "transferToken", "highq" };
        private List<string> parameterTypes = new List<string>() { "array", "date", "int", "object", "short", "string", "DateTime", "bool" };
        private List<string> requestMethodTypes = new List<string>() { "GET", "POST" };
        private List<string> requestInClauseTypes = new List<string>() { "Child", "Body", "OData", "Query", "Headers" };

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
                    if (string.IsNullOrEmpty(entityType) || connector.Info.EntityType.ToString() == entityType)
                    {
                        if (string.IsNullOrEmpty(subType) || connector.Info.SubType == subType)
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
                            on connector.Info.DataSourceId equals dataSource.Id
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
        /// Will check duplicate datasources
        /// </summary>
        [Fact]
        public async Task VerifyDuplicateDataSources()
        {
            var datasources = GetDataSources();
            // Assert
            Assert.False(datasources.GroupBy(x => x.Id).Any(g => g.Count() > 1),"Data source duplicate id found");
        }

        /// <summary>
        /// Will check duplicate connectors
        /// </summary>
        [Fact]
        public async Task VerifyDuplicateConnectors()
        {
            var connectors = GetConnectors(null, null);
            // Assert
            Assert.False(connectors.GroupBy(x => x.Id).Any(g => g.Count() > 1), "Connector duplicate id found");
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
            connector.PartitionKey.Should().Be("ConnectorList");
            connector.Info.Should().NotBeNull();
            connector.Info.Title.Should().NotBeNullOrWhiteSpace();
            connector.Info.Description.Should().NotBeNullOrWhiteSpace();
            connector.Info.EntityType.ToString().Should().NotBeNullOrWhiteSpace();
            connector.Info.DataSourceId.Should().NotBeNullOrWhiteSpace();
            connector.Info.Version.Should().NotBeNull();
            if (string.IsNullOrEmpty(connector.Info.SubType) || connector.Info.SubType == SubType.Parent)
            {
                connector.Alias.Should().NotBeNull();
                connector.Request.Should().NotBeNull();
                connector.Request.EndpointPath.Should().NotBeNull();
                connector.Request.EndpointPath.Should().StartWith("/");
                connector.Request.Method.Should().ContainAny(requestMethodTypes);
                foreach (var param in connector.Request.Parameters)
                {
                    if (param.DefaultValue.ToString().StartsWith("{"))
                    {
                        param.CdmName.Should().BeNullOrWhiteSpace();
                    }
                    else
                    {
                        param.CdmName.Should().NotBeNullOrWhiteSpace();
                    }
                    param.Name.Should().NotBeNullOrWhiteSpace();
                    param.InClause.Should().ContainAny(requestInClauseTypes);
                    param.Type.Should().ContainAny(parameterTypes);
                }
                foreach (var sortParam in connector.Request.Sorting?.Properties)
                {
                    sortParam.Name.Should().NotBeNullOrWhiteSpace();
                    sortParam.Element.Should().NotBeNull();
                    sortParam.ElementValue.Should().NotBeNull();
                    sortParam.Type.Should().ContainAny(parameterTypes);
                }
                if (connector.Response.Type == Data.Model.ResponseContentType.XML.ToString())
                {
                    connector.Response.XmlArrayAttribute.Should().NotBeNull();
                }
                foreach (var filterValue in connector.Filters)
                {
                    filterValue.Name.Should().NotBeNullOrWhiteSpace();
                    filterValue.Description.Should().NotBeNullOrWhiteSpace();
                    filterValue.RequestParameter.Should().NotBeNull();
                    if (filterValue.FilterList != null)
                    {
                        foreach (var filter in filterValue.FilterList)
                        {
                            filter.Name.Should().NotBeNullOrWhiteSpace();
                            filter.Id.Should().NotBeNullOrWhiteSpace();
                        }
                    }
                }
            }
            else
            {
                connector.Alias.Should().BeNull();
                connector.Request.EndpointPath.Should().BeNull();
                connector.Request.Method.Should().BeNull();
                connector.Request.Parameters.Should().BeNull();
                connector.Request.Sorting.Should().BeNull();
                connector.Response.XmlArrayAttribute.Should().BeNull();
                connector.Response.Schema.Should().BeNull();
                connector.Filters.Should().BeEmpty();
            }

            connector.CdmMapping.Should().NotBeNull();
            connector.CdmMapping.structured.Should().NotBeNull();
            foreach (var param in connector.CdmMapping.structured)
            {
                param.name.Should().NotBeNull();
                param.type.Should().ContainAny(parameterTypes);
                param.responseElement.Should().NotBeNull();

                // Verify child connector exists
                if (param.type == "object")
                {
                    var allEntityChildrenRecords = GetConnectors(connector.Info.EntityType.ToString(), "Child");
                    var match = Regex.Match(param.responseElement, @"{(.*?)}");
                    var connectionId = match.Groups[1].ToString();
                    var childConnector = allEntityChildrenRecords.SingleOrDefault(x => x.Id == connectionId);
                    childConnector.Should().NotBeNull();
                }
            }
            foreach (var param in connector.CdmMapping.unstructured)
            {
                //should be camelCased
                bool nameCamelCased = !char.IsUpper(param.name[0]);
                nameCamelCased.Should().BeTrue("Should be camel cased");
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
            if (connector.Info.SubType == SubType.Parent)
            {
                connector.Alias.Should().StartWith($"{dataSource.appCode}.{connector.Info.EntityType}".ToLower());
            }

        }
    }
}
