using System.Net;
using System.Threading.Tasks;
using Xunit;
using Zurich.Common.Exceptions;

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
            var request = "/Connectors/10";
            //Act
            var response = await _client.GetAsync(request);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestConnectorsById_With_NotFound()
        {
            // Arrange
            var request = "/Connectors/14";
            //Act
            var response = await _client.GetAsync(request);
            // var response = Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _client.GetAsync(request));
            await Task.Delay(5000);
            var message = response;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact]
        public async Task TestConnectorDataById_With_Statucode_OK()
        {
            // Arrange
            var request = "/Connectors/12/data";
            //Act
            var response= await _client.GetAsync(request);

            // Assert
           Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task TestConnectorDataById_With_SuccessStatus()
        {
            // Arrange
            var request = "/Connectors/10/data/?query=query";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TestConnectorData_for_DocumentType_As_Array_With_SuccessStatus()
        {
            // Arrange
            var request = "/Connectors/10/data/?query=test&retrieveFilters=true";
            //Act
            var response = await _client.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
