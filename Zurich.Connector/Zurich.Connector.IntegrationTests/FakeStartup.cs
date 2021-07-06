using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
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
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication("IntegrationTest")
                .AddScheme<AuthenticationSchemeOptions, IntegrationTestAuthenticationHandler>(
                  "IntegrationTest",
                  options => { }
                );
        }
    }
}
