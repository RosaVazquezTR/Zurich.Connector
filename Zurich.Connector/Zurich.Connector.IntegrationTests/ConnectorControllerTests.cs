using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Zurich.Connector.Web;

namespace Zurich.Connector.IntegrationTests
{
    public class ConnectorControllerTests : IntegrationTest
    {

        public ConnectorControllerTests(CustomWebApplicationFactory fixture):base(fixture)
        {
        }


        [Fact]
        public async Task GetConnectorConfigDataTest()
        {
            // Arrange
            var response = await _client.GetAsync("/Connectors");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);


        }
    }
}
