using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Zurich.Common.Exceptions;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Web;
using Zurich.Connector.Web.Controllers;

namespace Zurich.Connector.IntegrationTests
{
    public class ConnectorControllerTests : IntegrationTest
    {

        public ConnectorControllerTests(CustomWebApplicationFactory fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task TestConnectorsById()
        {
            // Arrange
            var request = "/Connectors/101";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestConnectorsById_With_NotFound()
        {
            // Arrange
            var request = "/Connectors/1123";
            // var sut = new ConnectorsController();
            //Act
            var response = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _client.GetAsync(request));
            await Task.Delay(5000);
            var message = response.Result.Message;

            // Assert
            Assert.Equal("Connector 'id' not found", message);
        }

        [Fact]
        public async Task TestConnectorDataById_With_Statucode_OK()
        {
            // Arrange
            var request = "/Connectors/1234/data";
            //Act
            var response= await _client.GetAsync(request);

            // Assert
           Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task TestConnectorDataById_With_SuccessStatus()
        {
            // Arrange
            var request = "/Connectors/1300/data/?query=query";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
