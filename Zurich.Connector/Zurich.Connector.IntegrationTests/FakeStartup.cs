﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Zurich.Common.Factories;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using Zurich.Common.Services.Security;
using Zurich.Common.Services.Security.CIAM;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data;
using Zurich.Connector.Web;
using Zurich.TenantData;
using System.Net.Http;
using Zurich.Common.Factories;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using Microsoft.Extensions.Logging;

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

            // Identity Server
            services.AddSingleton<ITokenAuthorityDiscoveryService, IdentityServerTokenAuthorityDiscoveryService>(s => new IdentityServerTokenAuthorityDiscoveryService(
                authOptions.TokenIssuer, s.GetRequiredService<ILogger<IdentityServerTokenAuthorityDiscoveryService>>(), s.GetRequiredService<IOIDCAuthorityRepo>(), s.GetRequiredService<IHttpClientFactory>()));
            // CIAM
            services.AddSingleton<ITokenAuthorityDiscoveryService, CIAMTokenAuthorityDiscoveryService>(s => new CIAMTokenAuthorityDiscoveryService(
                ciamOptions.TokenIssuer, s.GetRequiredService<ILogger<CIAMTokenAuthorityDiscoveryService>>(), s.GetRequiredService<IOIDCAuthorityRepo>(), s.GetRequiredService<IHttpClientFactory>()));
            services.AddScoped<ITokenDiscoveryServiceFactory, TokenDiscoveryServiceFactory>();
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
