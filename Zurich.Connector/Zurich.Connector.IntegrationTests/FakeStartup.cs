using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using Zurich.Common.Services.Security;
using Zurich.Common.Services.Security.CIAM;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data;
using Zurich.Connector.Web;
using Zurich.TenantData;

namespace Zurich.Connector.IntegrationTests
{
    public class TestInformation
    {
        public static bool columnEncryptionRan;
    }

    public class FakeStartup : Startup
    {
        public FakeStartup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
        }

        public override void AddAuthServices(IServiceCollection services, AuthIssuerOptions authOptions, CIAMAuthOptions ciamOptions)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddScoped<ISessionAccessor, IntegrationTestSessionAccessor>();
            services.AddScoped<ICosmosService, IntegrationTestCosmosService>();
            services.AddScoped<ILegalHomeAccessCheck, IntegrationTestLegalHomeAccess>();
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!TestInformation.columnEncryptionRan)
            {
                TestInformation.columnEncryptionRan = true;
                RegisterColumnEncryptionProvider(app);
            }
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
