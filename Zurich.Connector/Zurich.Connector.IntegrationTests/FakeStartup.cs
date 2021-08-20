using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using Zurich.Common;
using Zurich.Common.Models;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services;
using Zurich.Connector.Web;
using Zurich.TenantData;

namespace Zurich.Connector.IntegrationTests
{
    public class FakeStartup : Startup
    {
        private AzureAdOptions _azureOptions;
        private static OAuthOptions _oAuthOptions;
        private static MicroServiceOptions _microServOptions;
        private CosmosDbOptions _connectorCosmosDbOptions;
        private CosmosClientSettings _connectorCosmosClientOptions;
        public FakeStartup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
        }

        public override void AddAuthServices(IServiceCollection services, string authority)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication("IntegrationTest")
                .AddScheme<AuthenticationSchemeOptions, IntegrationTestAuthenticationHandler>(
                  "IntegrationTest",
                  options => { }
                );
            //services.AddScoped<ISessionAccessor, IntegrationTestSessionAccessor>();
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            RegisterColumnEncryptionProvider(app);
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}
