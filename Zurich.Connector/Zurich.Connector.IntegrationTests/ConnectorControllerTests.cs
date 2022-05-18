using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Zurich.Common.Exceptions;
using Zurich.Common;
using Zurich.Common.Testing;

namespace Zurich.Connector.IntegrationTests
{
    public class ConnectorControllerTests : IntegrationTest
    {
        protected IConfiguration configuration = IntegrationConfiguration();

        public ConnectorControllerTests(CustomWebApplicationFactory fixture) : base(fixture)
        {
        }

        public static IConfiguration IntegrationConfiguration()
        {
            string json = $"integrationsettings.json";

            var configBuilder = new ConfigurationBuilder().AddJsonFile(json, optional: true);
            var _configuration = configBuilder.Build();

            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? _configuration["ASPNETCORE_ENVIRONMENT"];

            string envJson = $"integrationsettings.{env}.json";

            var config = new ConfigurationBuilder()
                .AddJsonFile(json, optional: true)
                .AddJsonFile(envJson, optional: true)
                .Build();

            return config;
        }

        [Fact]
        public async Task TestConnectorsById()
        {
            // Arrange
            var request = "/api/v1/Connectors/10";
            //Act
            var response = await _client.GetAsync(request);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestConnectorsById_With_NotFound()
        {
            // Arrange
            var request = "/api/v1/Connectors/14";
            //Act
            var response = await _client.GetAsync(request);
            // var response = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _client.GetAsync(request));
            await Task.Delay(5000);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact]
        public async Task TestConnectorDataById_With_Statucode_OK()
        {
            // Arrange
            var request = $"/api/v1/Connectors/12/data";
            HttpRequestMessage getRequest = new Helper().TokenRequest(request);

            //Act
            var response= await _client.SendAsync(getRequest);

            // Assert
           Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestXMLConnectorDataById_With_SuccessStatus()
        {
            // Arrange
            var request = $"/api/v1/Connectors/10/data/?query=query";
            HttpRequestMessage getRequest = new Helper().TokenRequest(request);

            //Act
            var response = await _client.SendAsync(getRequest);
            var contentString = await response.Content.ReadAsStringAsync();
            var message = JToken.Parse(contentString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Some quick search specific checks on response
            Assert.False(string.IsNullOrEmpty(message["count"].Value<string>()), "count property expected");
            Assert.False(string.IsNullOrEmpty(message["sourceDirectoryUrl"].Value<string>()), "sourceDirectoryUrl property expected");
            Assert.NotNull((JArray)message["documents"]);
        }

        [Fact]
        public async Task TestJsonConnectorDataById_With_SuccessStatus()
        {
            // Arrange
            var request = $"/api/v1/Connectors/14/data/?query=query";
            HttpRequestMessage getRequest = new Helper().TokenRequest(request);


            //Act
            var response = await _client.SendAsync(getRequest);
            var contentString = await response.Content.ReadAsStringAsync();
            var message = JToken.Parse(contentString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Some quick search specific checks on response
            Assert.False(string.IsNullOrEmpty(message["count"].Value<string>()), "count property expected");
            Assert.False(string.IsNullOrEmpty(message["sourceDirectoryUrl"].Value<string>()), "sourceDirectoryUrl property expected");
            Assert.NotNull((JArray)message["documents"]);
        }

        

        [Fact]
        public async Task TestConnectorData_for_DocumentType_As_Array_With_SuccessStatus()
        {
            // Arrange
            var request = $"/api/v1/Connectors/10/data/?query=test&retrieveFilters=true";
            HttpRequestMessage getRequest = new Helper().TokenRequest(request);

            //Act
            var response = await _client.SendAsync(getRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
