using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.IntegrationTests
{

    /// <summary>
    /// Not really integration tests but will pull the json files and verify things look appropriate.
    /// </summary>
    public class ConnectorTests : IntegrationTest
    {
        private const string folderLocation = @"..\..\..\..\..\Zurich.Connector.Deploy\CosmosRecords";


        public ConnectorTests(CustomWebApplicationFactory fixture) : base(fixture)
        {
        }

        public static List<ConnectorDocument> GetConnectors(string entityType, bool isParent)
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
                        if (!isParent || string.IsNullOrEmpty(connector.info.subType) || connector.info.subType == "Parent")
                        {
                            connectors.Add(connector);
                        }
                    }
                }
            }
            return connectors;
        }

        public static IEnumerable<object[]> GetConnectorsTestCases(string entityType, bool isParent)
        {
            var connectors = GetConnectors(entityType, isParent);
            IEnumerable<object[]> testCases = connectors.Select(x => new object[] { x });
            return testCases;
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "Document", true })]
        public async Task MakeDocumentCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
            //var document = JsonConvert.DeserializeObject<Document>(responseContent);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "Favorites", true })]
        public async Task MakeFavoriteCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "History", true })]
        public async Task MakeHistoryCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "Search", true })]
        public async Task MakeSearchCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "UserProfile", true })]
        public async Task MakeUserProfileCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
        }
    }
}
