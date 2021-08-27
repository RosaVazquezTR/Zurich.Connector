using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Zurich.Common.Models.CommonDataModels;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.IntegrationTests.Models;

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

        private static async Task CheckResponse<T>(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
            var jToken = JToken.Parse(responseContent);
            if (!(jToken is JArray) || ((JObject)jToken).ContainsKey("hResult"))
            {
                var errorResponse = JsonConvert.DeserializeObject<DataError>(responseContent);
                errorResponse.Message.Should().BeNullOrWhiteSpace();
            }
            else
            {
                var jsonResponse = JsonConvert.DeserializeObject<T>(jToken.ToString());
            }
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "Document", true })]
        public async Task MakeDocumentCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}/Data";
            // TODO: remove when we can find host and dont have to pass in
            if (connector.info.dataSourceId == "10")
            {
                request = $"/Connectors/{connector.Id}/Data?Hostname=2fdb2-dmobility.imanage.work";
            }
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            await CheckResponse<List<DocumentEntity>>(response);
        }

        // Will need to create transfer token so these can't be turned on yet.
        /*[Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "Favorites", true })]
        public async Task MakeFavoriteCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}/Data";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            await CheckResponse<List<FavoritiesEntity>>(response);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "History", true })]
        public async Task MakeHistoryCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}/Data";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            await CheckResponse<List<HistoryEntity>>(response);
        }*/

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "Search", true })]
        public async Task MakeSearchCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}/Data?Query=*";

            //Act
            var response = await _client.GetAsync(request);

            // Assert
            await CheckResponse<SearchObject>(response);
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "UserProfile", true })]
        public async Task MakeUserProfileCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/Connectors/{connector.Id}/Data";
            // TODO: remove when we can find host and dont have to pass in
            if (connector.info.dataSourceId == "10")
            {
                request = $"/Connectors/{connector.Id}/Data?Hostname=2fdb2-dmobility.imanage.work";
            }

            //Act
            var response = await _client.GetAsync(request);

            // Assert
            await CheckResponse<UserProfileEntity>(response);
        }
    }
}
