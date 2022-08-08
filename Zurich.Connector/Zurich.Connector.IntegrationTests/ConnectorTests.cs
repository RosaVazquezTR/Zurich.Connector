using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Zurich.Common.Models.CommonDataModels;
using Zurich.Common;
using Zurich.Connector.App;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.IntegrationTests.Models;
using Zurich.Common.Testing;

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
                    if (string.IsNullOrEmpty(entityType) || connector.Info.EntityType.ToString() == entityType)
                    {
                        if (!isParent || string.IsNullOrEmpty(connector.Info.SubType) || connector.Info.SubType == SubType.Parent)
                        {
                            //ToDo: Needs to remove this once iManage connector token generation sucessful.
                            if (connector.Id != "44")
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
            var request = $"/api/v1/Connectors/{connector.Id}/Data";
            // TODO: remove when we can find host and dont have to pass in
            if (connector.Info.DataSourceId == "10")
            {
                request = $"http://localhost//api/v1/Connectors/{connector.Id}/Data?Hostname=cloudimanage.com";
            }
            if (connector.Info.DataSourceId != "10" && connector.Info.DataSourceId != "45")
            {
                HttpRequestMessage getRequest = new Helper().TokenRequest(request);

                //Act
                var response = await _client.SendAsync(getRequest);

                // Assert
                await CheckResponse<List<DocumentEntity>>(response);
            }
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
            

            // Note:- Workaround to skip HighQ connector, TRMarketplace (29) check
            //        and MS Graph External Search Connector (49) check (Test user didn't consent ExternalItem.Read therefore will get 403 forbidden on graph side)
            if (connector.Id != "29" &&  connector.Id != "47" && connector.Id != "48" && connector.Id != "49")
            {
                // Arrange
                var request = $"/api/v1/Connectors/{connector.Id}/Data?Query=*";

                HttpRequestMessage getRequest = new Helper().TokenRequest(request);

                //Act
                var response = await _client.SendAsync(getRequest);

                // Assert
                await CheckResponse<SearchObject>(response);
            }
        }
        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "Search", true })]
        public async Task MakeSearchCallsBasicAuth(ConnectorDocument connector)
        {
            //At the moment, TRMarketplace (id 29) is the only connector that uses basic auth.
            //Some othere connectors that use that authentication should be tested in here.
            if (connector.Id == "29")
            {
                // Arrange
                var request = $"/api/v1/Connectors/{connector.Id}/Data?Query=site&text=site&fields=%5B%22name%22%5D";

                HttpRequestMessage getRequest = new Helper().TokenRequest(request);

                //Act
                var response = await _client.SendAsync(getRequest);

                // Assert
                await CheckResponse<SearchObject>(response);
            }
        }

        [Theory]
        [MemberData(nameof(GetConnectorsTestCases), parameters: new object[] { "UserProfile", true })]
        public async Task MakeUserProfileCalls(ConnectorDocument connector)
        {
            // Arrange
            var request = $"/api/v1/Connectors/{connector.Id}/Data";
            // TODO: remove when we can find host and dont have to pass in
            if (connector.Info.DataSourceId == "10" && connector.Info.DataSourceId == "42")
            {
                request = $"/api/v1/Connectors/{connector.Id}/Data?Hostname=cloudimanage.com";
            }

            if (connector.Info.DataSourceId != "10" && connector.Info.DataSourceId == "42")
            {
                //Act
                var response = await _client.GetAsync(request);

                // Assert
                await CheckResponse<UserProfileEntity>(response);
            }
        }
    }
}
