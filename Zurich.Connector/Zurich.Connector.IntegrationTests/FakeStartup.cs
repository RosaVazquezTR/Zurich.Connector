using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zurich.Connector.Web;

namespace Zurich.Connector.IntegrationTests
{
    public class FakeStartup : Startup
    {
        public FakeStartup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
        }

        public override void AddAuthServices(IServiceCollection services, string audience, string authority)
        {
            //services.AddAuthentication("IntegrationTest")
            //    .AddScheme<AuthenticationSchemeOptions, IntegrationTestAuthenticationHandler>(
            //      "IntegrationTest",
            //      options => { }
            //    );
        }
    }
}
