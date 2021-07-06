using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
            _client = _factory.CreateClient();
        }
    }
}
