using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace Zurich.Connector.IntegrationTests
{
    public abstract class IntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {

        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;

        public IntegrationTest(CustomWebApplicationFactory fixture)
        {
            _factory = fixture;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("IntegrationTest");
        }
    }
}
