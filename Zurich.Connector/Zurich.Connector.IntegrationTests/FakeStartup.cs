using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using Zurich.Connector.Web;
using Zurich.TenantData;

namespace Zurich.Connector.IntegrationTests
{
    public class FakeStartup : Startup
    {
        private bool _columnEncryptionRan;

        public FakeStartup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
        }

        public override void AddAuthServices(IServiceCollection services, string authority)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddScoped<ISessionAccessor, IntegrationTestSessionAccessor>();
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!_columnEncryptionRan)
            {
                RegisterColumnEncryptionProvider(app);
                _columnEncryptionRan = true;
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
